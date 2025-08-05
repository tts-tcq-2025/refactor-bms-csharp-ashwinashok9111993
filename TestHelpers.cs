using System.Collections.Generic;

public static class TestHelpers
{
    public static (List<string> capturedOutput, OutputWriter mockWriter) CreateMockOutputWriter()
    {
        var capturedOutput = new List<string>();
        OutputWriter mockWriter = message => capturedOutput.Add(message);
        return (capturedOutput, mockWriter);
    }
}
