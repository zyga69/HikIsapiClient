namespace Zyga.HikIsapiClient.Tests.TestSupport;

/// <summary>
/// Canned ISAPI XML response bodies used to drive <see cref="FakeIsapiServer"/> in tests.
/// </summary>
internal static class IsapiXml
{
    public const string LoginCapabilities =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <SessionLoginCap xmlns="http://www.isapi.org/ver20/XMLSchema">
          <sessionID>sess-1</sessionID>
          <challenge>chal-1</challenge>
          <iterations>1</iterations>
          <isIrreversible>false</isIrreversible>
          <salt>salt-1</salt>
          <sessionIDVersion>2</sessionIDVersion>
        </SessionLoginCap>
        """;

    public const string LoginSuccess =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <SessionLoginCap xmlns="http://www.isapi.org/ver20/XMLSchema">
          <statusValue>1</statusValue>
          <statusString>OK</statusString>
          <isDefaultPassword>false</isDefaultPassword>
          <isRiskPassword>false</isRiskPassword>
          <isActivated>true</isActivated>
        </SessionLoginCap>
        """;

    public const string ResponseStatusOk =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <ResponseStatus xmlns="http://www.std-cgi.com/ver10/XMLSchema">
          <statusCode>1</statusCode>
          <statusString>OK</statusString>
        </ResponseStatus>
        """;

    public const string ResponseStatusInvalidOperation =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <ResponseStatus xmlns="http://www.std-cgi.com/ver10/XMLSchema">
          <statusCode>2</statusCode>
          <statusString>Invalid Operation</statusString>
        </ResponseStatus>
        """;
}
