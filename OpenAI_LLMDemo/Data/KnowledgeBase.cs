namespace OpenAI_LLMDemo.Data;

// Our "documents". The AI has never seen this information (Sunrise Clinic is made up),
// so if it answers correctly, it must have used RAG.
public static class KnowledgeBase
{
    public static readonly List<string> Documents =
    [
        "Sunrise Clinic is open Monday to Friday from 8 AM to 6 PM, and Saturday from 9 AM to 1 PM. It is closed on Sundays.",
        "Patients can book an appointment by calling 080-1234-5678 or through the Sunrise patient portal.",
        "To cancel an appointment, patients must inform the clinic at least 24 hours before. Late cancellations are charged 200 rupees.",
        "A general consultation with a doctor costs 500 rupees. A follow-up visit within 7 days is free.",
        "Dr. Priya Sharma is the cardiologist (heart specialist). She sees patients on Tuesdays and Thursdays.",
        "Dr. Arjun Rao is the pediatrician and treats children up to 16 years old. He is available every weekday morning.",
        "Lab test results are uploaded to the patient portal within 48 hours. Urgent results are phoned to the patient the same day.",
        "The clinic accepts cash, UPI, debit cards and credit cards. Insurance claims are supported for Star Health and HDFC Ergo.",
        "Free parking for patients is available in the basement. Two-wheeler parking is near the main gate.",
        "For a medical emergency outside clinic hours, call 108 or go to City General Hospital, 2 km away.",
        "On the first visit, patients should bring a photo ID and their previous prescriptions.",
        "Flu vaccination is available every October and November without an appointment."
    ];
}
