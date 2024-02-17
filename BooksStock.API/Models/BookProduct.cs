/*
 * 
 * Entity Model for Books Collection in MongoDB Atlas
 */

using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BooksStock.API.Models
{
    public class BookProduct
    {
        [BsonId]
        [ReadOnly(true)]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("book")]
        public string? Title { get; set; }

        [BsonElement("author")]
        public string? Author { get; set; }

        [BsonElement("annotation")]
        public string? Description { get; set; }

        [BsonElement("language")]
        public string? Language { get; set; }

        [BsonElement("genre")]
        public string[] ? Genres { get; set; }

        [BsonElement("link")]
        public Uri? Link { get; set; } = new Uri("about:blank");

        [BsonElement("available")]
        public bool IsAvailable { get; set; }

        [BsonElement("price")]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }
    }
}
