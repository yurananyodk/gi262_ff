using System;
using Newtonsoft.Json;
using UnityEditor.Search;
using UnityEditor.TestTools.TestRunner.Api;

[System.Serializable]
public class TestResult
{
    [JsonProperty("testName")]
    public string TestName { get; set; }

    [JsonProperty("fullName")]
    public string FullName { get; set; }

    [JsonProperty("resultState")]
    public string ResultState { get; set; }

    [JsonProperty("testStatus")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public TestStatus TestStatus { get; set; }

    [JsonProperty("duration")]
    public double Duration { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("stackTrace")]
    public string StackTrace { get; set; }

    [JsonProperty("namespace")]
    public string Namespace { get; set; }

    [JsonProperty("startTime")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime EndTime { get; set; }
}

public class TestSummary
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }
    [JsonProperty("passedCount")]
    public int PassedCount { get; set; }
    [JsonProperty("failedCount")]
    public int FailedCount { get; set; }
    [JsonProperty("skippedCount")]
    public int SkippedCount { get; set; }
}
[System.Serializable]
public class TestRunResult
{
    [JsonProperty("testRunInfo")]
    public TestRunInfo TestRunInfo;

    [JsonProperty("summary")]
    public TestSummaryJson Summary;

    [JsonProperty("testResults")]
    public TestResult[] TestResults;

    [JsonProperty("testCaseFilesHash")]
    public string TestCaseFilesHash;
}
[System.Serializable]
public class TestRunInfo
{
    [JsonProperty("startTime")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime StartTime;

    [JsonProperty("endTime")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime EndTime;

    [JsonProperty("duration")]
    public double Duration;

    [JsonProperty("testSuite")]
    public string TestSuite;
}
[System.Serializable]
public class TestSummaryJson
{
    [JsonProperty("totalCount")]
    public int TotalCount;

    [JsonProperty("passedCount")]
    public int PassedCount;

    [JsonProperty("failedCount")]
    public int FailedCount;

    [JsonProperty("skippedCount")]
    public int SkippedCount;

    [JsonProperty("successRate")]
    public double SuccessRate;
}