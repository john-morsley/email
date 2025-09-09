//using System.ComponentModel.DataAnnotations;
//using Morsley.UK.Email.Common.Attributes;

//namespace Morsley.UK.Email.Common.Models;

//public class SendableEmailMessage
//{
//    [Required(ErrorMessage = "At least one recipient is required")]
//    [MinLength(1, ErrorMessage = "At least one recipient is required")]
//    [EmailListValidation]
//    public List<string> To { get; set; } = new();

//    [EmailListValidation]
//    public List<string> Cc { get; set; } = new();

//    [EmailListValidation]
//    public List<string> Bcc { get; set; } = new();

//    [Required(ErrorMessage = "Subject is required")]
//    [StringLength(998, ErrorMessage = "Subject cannot exceed 998 characters")]
//    public string Subject { get; set; } = "";

//    [StringLength(1000000, ErrorMessage = "Text body cannot exceed 1MB")]
//    public string? TextBody { get; set; }

//    [StringLength(1000000, ErrorMessage = "HTML body cannot exceed 1MB")]
//    public string? HtmlBody { get; set; }
//}