using System.ComponentModel.DataAnnotations;

namespace OpenAI_LLMDemo.Models;

public class AskRequest
{
    [Required]
    public string Question { get; set; } = "";
}
