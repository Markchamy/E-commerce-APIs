using Backend.Interfaces;
using Backend.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml.Linq;

namespace Backend.Repositories
{
    public class OdooXmlRpcService : IOdooXmlRpcService
    {
        private readonly HttpClient _httpClient;
        private readonly OdooOptions _options;
        private readonly ILogger<OdooXmlRpcService> _logger;
        private int? _cachedUid;

        public OdooXmlRpcService(HttpClient httpClient, IOptions<OdooOptions> options, ILogger<OdooXmlRpcService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int?> AuthenticateAsync()
        {
            if (_cachedUid.HasValue)
            {
                return _cachedUid.Value;
            }

            try
            {
                var xmlRequest = BuildXmlRpcRequest("authenticate", new object[]
                {
                    _options.Database,
                    _options.Username,
                    _options.Password,
                    new Dictionary<string, object>()
                });

                var response = await SendXmlRpcRequestAsync($"{_options.Url}/xmlrpc/2/common", xmlRequest);
                var uid = ParseIntResponse(response);

                if (uid.HasValue && uid.Value > 0)
                {
                    _cachedUid = uid;
                    _logger.LogInformation("Odoo authentication successful. UID: {Uid}", uid);
                }
                else
                {
                    _logger.LogWarning("Odoo authentication failed");
                }

                return uid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Odoo");
                return null;
            }
        }

        /// <summary>
        /// Calls create_general_pos_order_api on pos.order model
        /// Creates order, picking, makes picking done and marks order as paid
        /// </summary>
        public async Task<OdooCreateOrderResponse> CreateGeneralPosOrderAsync(OdooCreateOrderRequest request)
        {
            try
            {
                var uid = await AuthenticateAsync();
                if (!uid.HasValue)
                {
                    return new OdooCreateOrderResponse
                    {
                        Success = false,
                        Error = "Authentication failed"
                    };
                }

                // Build the order data dictionary matching the documented API
                var orderData = new Dictionary<string, object>
                {
                    { "invoice_ref", request.InvoiceRef },
                    { "return", request.Return },
                    { "mobile", request.Mobile ?? string.Empty },
                    { "voucher_code", request.VoucherCode ?? string.Empty },
                    { "code", request.Code },
                    { "order_date", request.OrderDate },
                    { "location_id", request.LocationId }
                };

                // Build products array
                var productsArray = new List<object>();
                foreach (var product in request.Products)
                {
                    var productDict = new Dictionary<string, object>
                    {
                        { "product_ref", product.ProductRef },
                        { "product_price", Convert.ToDouble(product.ProductPrice) },
                        { "quantity", product.Quantity }
                    };

                    // Add lot/codentify if provided
                    if (!string.IsNullOrEmpty(product.Lot))
                    {
                        productDict["lot"] = product.Lot;
                    }

                    productsArray.Add(productDict);
                }
                orderData["products"] = productsArray.ToArray();

                // Call create_general_pos_order_api via execute_kw
                // Odoo's execute_kw unpacks args as: ids, args = args[0], args[1:]
                // So for model methods: args[0] = [] (empty record IDs), args[1] = actual parameter
                // The method signature is: create_general_pos_order_api(self, kw_1) where kw_1 is the order dict
                var xmlRequest = BuildExecuteKwRequest(
                    "pos.order",
                    "create_general_pos_order_api",
                    new object[] { new object[] { }, orderData },  // [[], orderData] - empty IDs + order data
                    new Dictionary<string, object>()  // empty kwargs
                );

                _logger.LogInformation("Calling create_general_pos_order_api for invoice: {InvoiceRef}", request.InvoiceRef);

                var response = await SendXmlRpcRequestAsync($"{_options.Url}/xmlrpc/2/object", xmlRequest);
                var result = ParseResponse(response);

                // Check for error response
                if (result is Dictionary<string, object> dictResult)
                {
                    if (dictResult.TryGetValue("error", out var error))
                    {
                        return new OdooCreateOrderResponse
                        {
                            Success = false,
                            Error = error?.ToString()
                        };
                    }
                }

                // Success if result is true or a valid response
                if (result is bool boolResult && boolResult)
                {
                    return new OdooCreateOrderResponse
                    {
                        Success = true,
                        Message = "Order created successfully"
                    };
                }

                // If we got here with no error, assume success
                if (result != null)
                {
                    return new OdooCreateOrderResponse
                    {
                        Success = true,
                        Message = "Order created successfully"
                    };
                }

                return new OdooCreateOrderResponse
                {
                    Success = false,
                    Error = "Unknown error occurred"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling create_general_pos_order_api");
                return new OdooCreateOrderResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Calls user_creation_general on res.partner model
        /// Creates or updates a customer contact in Odoo
        /// </summary>
        public async Task<OdooUserCreationResponse> CreateUserGeneralAsync(OdooUserCreationRequest request)
        {
            try
            {
                var uid = await AuthenticateAsync();
                if (!uid.HasValue)
                {
                    return new OdooUserCreationResponse
                    {
                        Success = false,
                        Error = "Authentication failed"
                    };
                }

                // Build user data dictionary matching the documented API
                var userData = new Dictionary<string, object>
                {
                    { "mobile", request.Mobile },
                    { "code", request.Code },
                    { "firstname", request.Firstname },
                    { "terms_conditions", request.TermsConditions },
                    { "email_verified", request.EmailVerified },
                    { "opt_in", request.OptIn },
                    { "is_duty_free", request.IsDutyFree }
                };

                // Add optional fields
                if (!string.IsNullOrEmpty(request.Email))
                    userData["email"] = request.Email;

                if (!string.IsNullOrEmpty(request.Birthdate))
                    userData["birthdate"] = request.Birthdate;

                if (!string.IsNullOrEmpty(request.Lastname))
                    userData["lastname"] = request.Lastname;

                if (request.SourceChannel.HasValue)
                    userData["source_channel"] = request.SourceChannel.Value;

                if (request.RetailerId.HasValue)
                    userData["retailer_id"] = request.RetailerId.Value;

                if (!string.IsNullOrEmpty(request.Gender))
                    userData["gender"] = request.Gender;

                if (!string.IsNullOrEmpty(request.Address))
                    userData["address"] = request.Address;

                if (!string.IsNullOrEmpty(request.Country))
                    userData["country"] = request.Country;

                if (!string.IsNullOrEmpty(request.City))
                    userData["city"] = request.City;

                // Call user_creation_general via execute_kw
                // Odoo's execute_kw unpacks args as: ids, args = args[0], args[1:]
                // So for model methods: args[0] = [] (empty record IDs), args[1] = actual parameter
                var xmlRequest = BuildExecuteKwRequest(
                    "res.partner",
                    "user_creation_general",
                    new object[] { new object[] { }, userData },  // [[], userData] - empty IDs + user data
                    new Dictionary<string, object>()  // empty kwargs
                );

                _logger.LogInformation("Calling user_creation_general for mobile: {Mobile}", request.Mobile);

                var response = await SendXmlRpcRequestAsync($"{_options.Url}/xmlrpc/2/object", xmlRequest);
                var result = ParseResponse(response);

                // Check for error response
                if (result is Dictionary<string, object> dictResult)
                {
                    if (dictResult.TryGetValue("error", out var error))
                    {
                        return new OdooUserCreationResponse
                        {
                            Success = false,
                            Error = error?.ToString()
                        };
                    }

                    // Parse successful response
                    var userResponse = new OdooUserCreationResponse
                    {
                        Success = true,
                        Message = "User created successfully"
                    };

                    if (dictResult.TryGetValue("starter_kit", out var starterKit))
                        userResponse.StarterKit = Convert.ToInt32(starterKit);

                    if (dictResult.TryGetValue("consumer", out var consumer))
                        userResponse.Consumer = Convert.ToInt32(consumer);

                    if (dictResult.TryGetValue("stage", out var stage))
                        userResponse.Stage = stage?.ToString();

                    if (dictResult.TryGetValue("eligibility_list", out var eligibilityList) && eligibilityList is List<object> eligList)
                    {
                        userResponse.EligibilityList = new List<OdooEligibilityItem>();
                        foreach (var item in eligList)
                        {
                            if (item is Dictionary<string, object> eligItem)
                            {
                                userResponse.EligibilityList.Add(new OdooEligibilityItem
                                {
                                    Name = eligItem.TryGetValue("name", out var name) ? name?.ToString() ?? "" : "",
                                    InternalRef = eligItem.TryGetValue("internal_ref", out var internalRef) ? internalRef?.ToString() ?? "" : "",
                                    Eligibility = eligItem.TryGetValue("eligibility", out var eligibility) && Convert.ToBoolean(eligibility)
                                });
                            }
                        }
                    }

                    return userResponse;
                }

                return new OdooUserCreationResponse
                {
                    Success = false,
                    Error = "Unexpected response format"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling user_creation_general");
                return new OdooUserCreationResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Calls check_valid_codentify_general to validate codentify/serial numbers
        /// </summary>
        public async Task<OdooCodentifyValidationResponse> CheckValidCodentifyAsync(OdooCodentifyValidationRequest request)
        {
            try
            {
                var uid = await AuthenticateAsync();
                if (!uid.HasValue)
                {
                    return new OdooCodentifyValidationResponse
                    {
                        Success = false,
                        IsValid = false,
                        Error = "Authentication failed"
                    };
                }

                // Build products array for validation
                var productsArray = new List<object>();
                foreach (var product in request.Products)
                {
                    productsArray.Add(new Dictionary<string, object>
                    {
                        { "codentify", product.Codentify },
                        { "product_ref", product.ProductRef }
                    });
                }

                var productData = new Dictionary<string, object>
                {
                    { "products", productsArray.ToArray() }
                };

                // Call check_valid_codentify_general via execute_kw
                // Odoo's execute_kw unpacks args as: ids, args = args[0], args[1:]
                // So for model methods: args[0] = [] (empty record IDs), args[1] = actual parameter
                var xmlRequest = BuildExecuteKwRequest(
                    "stock.quant",
                    "check_valid_codentify_general",
                    new object[] { new object[] { }, productData },  // [[], productData] - empty IDs + product data
                    new Dictionary<string, object>()  // empty kwargs
                );

                _logger.LogInformation("Calling check_valid_codentify_general for {Count} products", request.Products.Count);

                var response = await SendXmlRpcRequestAsync($"{_options.Url}/xmlrpc/2/object", xmlRequest);
                var result = ParseResponse(response);

                // Check for error response
                if (result is Dictionary<string, object> dictResult)
                {
                    if (dictResult.TryGetValue("error", out var error))
                    {
                        return new OdooCodentifyValidationResponse
                        {
                            Success = false,
                            IsValid = false,
                            Error = error?.ToString()
                        };
                    }
                }

                // Result should be boolean (True if valid, False if not)
                if (result is bool boolResult)
                {
                    return new OdooCodentifyValidationResponse
                    {
                        Success = true,
                        IsValid = boolResult,
                        Message = boolResult ? "Codentify is valid" : "Codentify is not valid"
                    };
                }

                return new OdooCodentifyValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Error = "Unexpected response format"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling check_valid_codentify_general");
                return new OdooCodentifyValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Calls get_external_profile on res.partner model
        /// Checks if a customer exists in Odoo by email
        /// Java pattern: asListUser passes struct {"":""} as args and {"email": email} as kwargs
        /// </summary>
        public async Task<OdooExternalProfileResponse> GetExternalProfileAsync(OdooExternalProfileRequest request)
        {
            try
            {
                var uid = await AuthenticateAsync();
                if (!uid.HasValue)
                {
                    return new OdooExternalProfileResponse
                    {
                        Success = false,
                        CustomerExists = false,
                        Error = "Authentication failed"
                    };
                }

                // Java uses asListUser which passes:
                // args = {"": ""} (struct with empty key/value - placeholder)
                // kwargs = {"email": email}
                var args = new Dictionary<string, object> { { "", "" } };
                var kwargs = new Dictionary<string, object> { { "email", request.Email } };

                var xmlRequest = BuildExecuteKwRequest(
                    "res.partner",
                    "get_external_profile",
                    args,
                    kwargs
                );

                _logger.LogInformation("Calling get_external_profile for email: {Email}", request.Email);

                var response = await SendXmlRpcRequestAsync($"{_options.Url}/xmlrpc/2/object", xmlRequest);
                var result = ParseResponse(response);

                _logger.LogInformation("get_external_profile raw result: {Result}", result);

                if (result is Dictionary<string, object> dictResult)
                {
                    // Check for XML-RPC level error
                    if (dictResult.TryGetValue("error", out var error))
                    {
                        return new OdooExternalProfileResponse
                        {
                            Success = false,
                            CustomerExists = false,
                            Error = error?.ToString()
                        };
                    }

                    // Check the "state" field from Odoo response
                    var state = dictResult.TryGetValue("state", out var stateVal) ? stateVal?.ToString() : null;

                    if (state == "error")
                    {
                        // User does NOT exist in Odoo
                        return new OdooExternalProfileResponse
                        {
                            Success = true,
                            State = "error",
                            CustomerExists = false
                        };
                    }

                    // User EXISTS in Odoo - extract mobile from message
                    string? mobile = null;
                    if (dictResult.TryGetValue("message", out var messageVal) && messageVal is Dictionary<string, object> messageDict)
                    {
                        if (messageDict.TryGetValue("mobile", out var mobileVal))
                        {
                            mobile = mobileVal?.ToString();
                        }
                    }

                    return new OdooExternalProfileResponse
                    {
                        Success = true,
                        State = state,
                        CustomerExists = true,
                        Mobile = mobile
                    };
                }

                return new OdooExternalProfileResponse
                {
                    Success = false,
                    CustomerExists = false,
                    Error = "Unexpected response format"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling get_external_profile");
                return new OdooExternalProfileResponse
                {
                    Success = false,
                    CustomerExists = false,
                    Error = ex.Message
                };
            }
        }

        #region XML-RPC Helpers

        private string BuildXmlRpcRequest(string method, object[] parameters)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<methodCall>");
            sb.AppendLine($"  <methodName>{method}</methodName>");
            sb.AppendLine("  <params>");

            foreach (var param in parameters)
            {
                sb.AppendLine("    <param>");
                sb.AppendLine($"      {SerializeValue(param)}");
                sb.AppendLine("    </param>");
            }

            sb.AppendLine("  </params>");
            sb.AppendLine("</methodCall>");

            return sb.ToString();
        }

        private string BuildExecuteKwRequest(string model, string method, object args, Dictionary<string, object> kwargs)
        {
            var parameters = new object[]
            {
                _options.Database,
                _cachedUid ?? 0,
                _options.Password,
                model,
                method,
                args,
                kwargs
            };

            return BuildXmlRpcRequest("execute_kw", parameters);
        }

        private string SerializeValue(object value)
        {
            if (value == null)
            {
                return "<value><nil/></value>";
            }

            switch (value)
            {
                case string s:
                    return $"<value><string>{EscapeXml(s)}</string></value>";

                case int i:
                    return $"<value><int>{i}</int></value>";

                case long l:
                    return $"<value><int>{l}</int></value>";

                case double d:
                    return $"<value><double>{d.ToString(System.Globalization.CultureInfo.InvariantCulture)}</double></value>";

                case decimal dec:
                    return $"<value><double>{dec.ToString(System.Globalization.CultureInfo.InvariantCulture)}</double></value>";

                case bool b:
                    return $"<value><boolean>{(b ? 1 : 0)}</boolean></value>";

                case object[] arr:
                    var arrSb = new StringBuilder();
                    arrSb.Append("<value><array><data>");
                    foreach (var item in arr)
                    {
                        arrSb.Append(SerializeValue(item));
                    }
                    arrSb.Append("</data></array></value>");
                    return arrSb.ToString();

                case Dictionary<string, object> dict:
                    var dictSb = new StringBuilder();
                    dictSb.Append("<value><struct>");
                    foreach (var kvp in dict)
                    {
                        dictSb.Append("<member>");
                        dictSb.Append($"<name>{EscapeXml(kvp.Key)}</name>");
                        dictSb.Append(SerializeValue(kvp.Value));
                        dictSb.Append("</member>");
                    }
                    dictSb.Append("</struct></value>");
                    return dictSb.ToString();

                case List<object> list:
                    return SerializeValue(list.ToArray());

                default:
                    return $"<value><string>{EscapeXml(value.ToString() ?? "")}</string></value>";
            }
        }

        private string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private async Task<string> SendXmlRpcRequestAsync(string url, string xmlRequest)
        {
            _logger.LogDebug("Sending XML-RPC request to {Url}", url);
            var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private int? ParseIntResponse(string xmlResponse)
        {
            try
            {
                var doc = XDocument.Parse(xmlResponse);
                var valueElement = doc.Descendants("value").FirstOrDefault();

                if (valueElement != null)
                {
                    var intElement = valueElement.Element("int") ?? valueElement.Element("i4");
                    if (intElement != null && int.TryParse(intElement.Value, out var result))
                    {
                        return result;
                    }

                    // Check for fault
                    var faultString = doc.Descendants("faultString").FirstOrDefault();
                    if (faultString != null)
                    {
                        _logger.LogError("XML-RPC Fault: {Fault}", faultString.Value);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing XML-RPC response");
                return null;
            }
        }

        private object? ParseResponse(string xmlResponse)
        {
            try
            {
                var doc = XDocument.Parse(xmlResponse);

                // Check for fault first
                var faultElement = doc.Descendants("fault").FirstOrDefault();
                if (faultElement != null)
                {
                    // XML-RPC fault structure:
                    // <fault><value><struct>
                    //   <member><name>faultString</name><value><string>error message</string></value></member>
                    //   <member><name>faultCode</name><value><int>1</int></value></member>
                    // </struct></value></fault>

                    var faultValueElement = faultElement.Element("value");
                    string? faultString = null;

                    if (faultValueElement != null)
                    {
                        var faultData = ParseValue(faultValueElement);
                        if (faultData is Dictionary<string, object> faultDict)
                        {
                            if (faultDict.TryGetValue("faultString", out var faultStr))
                            {
                                faultString = faultStr?.ToString();
                            }
                        }
                    }

                    _logger.LogError("XML-RPC Fault: {Fault}", faultString ?? "Unknown fault");
                    return new Dictionary<string, object> { { "error", faultString ?? "Unknown fault" } };
                }

                var valueElement = doc.Descendants("param").FirstOrDefault()?.Element("value");
                if (valueElement != null)
                {
                    return ParseValue(valueElement);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing XML-RPC response");
                return null;
            }
        }

        private object ParseValue(XElement valueElement)
        {
            var child = valueElement.Elements().FirstOrDefault();
            if (child == null)
            {
                return valueElement.Value;
            }

            switch (child.Name.LocalName)
            {
                case "int":
                case "i4":
                    return int.TryParse(child.Value, out var intVal) ? intVal : 0;

                case "double":
                    return double.TryParse(child.Value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var doubleVal) ? doubleVal : 0.0;

                case "boolean":
                    return child.Value == "1";

                case "string":
                    return child.Value;

                case "array":
                    var dataElement = child.Element("data");
                    if (dataElement != null)
                    {
                        var list = new List<object>();
                        foreach (var item in dataElement.Elements("value"))
                        {
                            list.Add(ParseValue(item));
                        }
                        return list;
                    }
                    return new List<object>();

                case "struct":
                    var dict = new Dictionary<string, object>();
                    foreach (var member in child.Elements("member"))
                    {
                        var name = member.Element("name")?.Value ?? "";
                        var value = member.Element("value");
                        if (value != null)
                        {
                            dict[name] = ParseValue(value);
                        }
                    }
                    return dict;

                case "nil":
                    return null!;

                default:
                    return child.Value;
            }
        }

        #endregion
    }
}
