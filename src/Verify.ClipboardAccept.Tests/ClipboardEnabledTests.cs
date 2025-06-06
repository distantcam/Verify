﻿public class ClipboardEnabledTests
{
    [Fact]
    public void ParseEnvironmentVariable()
    {
        Assert.False(ClipboardEnabled.ParseEnvironmentVariable(null));
        Assert.False(ClipboardEnabled.ParseEnvironmentVariable("false"));
        Assert.True(ClipboardEnabled.ParseEnvironmentVariable("true"));
    }

    void EnableClipboard() =>
    #region EnableClipboard
        ClipboardAccept.Enable();
    #endregion

    [Fact]
    public Task ParseEnvironmentVariable_failure() =>
        Throws(() => ClipboardEnabled.ParseEnvironmentVariable("foo"));
}