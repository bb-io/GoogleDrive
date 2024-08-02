using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.GoogleDrive.DataSourceHandler.EnumHandlers;

public class MimeTypeDataHandler : IStaticDataSourceHandler
{ 
    public Dictionary<string, string> GetData()
    {
        return new Dictionary<string, string>
        {
            { "application/vnd.google-apps.spreadsheet", "Spreadsheet" },
            { "text/xml", "XML" },
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Word" },
            { "application/json", "JSON" },
            { "application/octet-stream", "Binary" },
            { "text/html", "HTML" },
            { "text/plain", "Text" },
            { "image/jpeg", "JPEG"},
            { "image/png", "PNG"},
            { "image/gif", "GIF"},
            { "image/bmp", "BMP"},
            { "application/msword", "Word"},
            { "application/vnd.ms-excel", "Excel"},
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Excel"},
            { "application/pdf", "PDF"},
            { "application/x-httpd-php", "PHP"},
            { "text/js", "JavaScript"},
            { "application/x-shockwave-flash", "Flash"},
            { "audio/mpeg", "MP3"},
            { "application/zip", "ZIP"},
            { "application/rar", "RAR"},
            { "application/tar", "TAR"},
            { "application/arj", "ARJ"},
            { "application/cab", "CAB"}
        };
    }
}