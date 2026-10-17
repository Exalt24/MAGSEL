// The only place the client's server addresses live. Both defaults point at a service
// running on this machine (see "Run it" in the README); change them here to use a
// hosted copy.
public static class ApiConfig
{
    // Flask AI service: Metaverse Files/appp.py (/predict, /generate_quiz, /generate_topic_chunk)
    public const string AiServiceBaseUrl = "http://localhost:5002";

    // Express accounts backend: Backend/ (/user/register, /user/login)
    public const string AccountsBaseUrl = "http://localhost:3000";
}
