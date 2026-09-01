namespace PlaywrightNUnitFramework.TestData{

public static class LoginTestData
{
    public static object[] Users =
    {
        new object[] { "standard_user", "secret_sauce", true },
        new object[] { "locked_out_user", "secret_sauce", false },
        new object[] { "invalid_user", "wrong_password", false }
    };
}
}
