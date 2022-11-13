namespace common.Database.Model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class CourierLocation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id{get;set;}
    [BsonRepresentation(BsonType.ObjectId)]
    public string CouirierId{get;set;}
    public decimal Latitude{get;set;}
    public decimal Longitude{get;set;}
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;
}
