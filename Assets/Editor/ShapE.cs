namespace AIComponent.ShapE
{
    public static class Api
    {
        public static string GetUrl()
        {
            var settings = AIComponentSettings.instance;
            return settings.shapeAPIBaseUrl + "/txt2blend";
        }

    } 

    
    [System.Serializable]
    public struct Response
    {
        public string download_url;
    }
}