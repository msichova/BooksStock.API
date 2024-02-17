/*
 * 
 * Model for filtering Books by entered parameters
 */
using System.ComponentModel.DataAnnotations;

namespace BooksStock.API.Models
{
    public class FilterForBook
    {
        public string? Title {  get; set; }
        public string? Author {  get; set; }
        public string? Annotation { get; set; }
        public string? Language { get; set; }
        public string[]? Genres { get; set; }
        public bool? IsAvailable { get; set; }

        [Range(0, int.MaxValue)]
        public decimal? MinPrice { get; set;}

        [Range(0, int.MaxValue)]
        public decimal? MaxPrice { get; set; }
        
    }
}
