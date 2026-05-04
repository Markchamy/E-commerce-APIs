namespace Backend.Models
{
    public class CollectionDTO
    {
        public int Id { get; set; }
        public string handle { get; set; }
        public string title { get; set; }
        public string? body_html { get; set; }
        public string sort_order { get; set; }
        public string? template_suffix { get; set; }
        public bool disjunctive { get; set; }
        public string published_scope { get; set; }
        public bool menu_category { get; set; }
        public int? Layer { get; set; }
        public List<RuleDTO> Rules { get; set; }
        public List<CollectionImageDTO> CollectionImages { get; set; }
    }
    public class RuleDTO
    {
        public int Id { get; set; }
        public string? column_name { get; set; }
        public string? relation { get; set; }
        public string? condition_text { get; set; }
    }
    public class CollectionImageDTO
    {
        public int Id { get; set;  }
        public string? src { get; set; }
    }
}
