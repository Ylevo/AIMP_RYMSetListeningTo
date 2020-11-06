using System;
using System.Diagnostics;
using System.Windows.Input;
using AIMP.SDK;
using AIMP.SDK.ActionManager;
using AIMP.SDK.MessageDispatcher;
using AIMP.SDK.Playlist;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.Reflection;

namespace AIMP_RYMSetListeningTo
{
    public delegate ActionResultType HookMessage(AimpCoreMessageType message, int param1, int param2);
    public class MessageHook : IAimpMessageHook
    {
        public AimpActionResult CoreMessage(AimpCoreMessageType message, int param1, int param2)
        {
            OnCoreMessage?.Invoke(message, param1, param2);
            return new AimpActionResult(ActionResultType.OK);
        }

        public event HookMessage OnCoreMessage;
    }


    [AimpPlugin("RYM Set Listening To", "Ylev", "1", AimpPluginType = AimpPluginType.Addons)]
    public class Program : AimpPlugin
    {
        private MessageHook hook;
        private Configuration config;
        private int intervalBetweenRequests = 15000;
        private string authToken, currentAlbum, currentAlbumRymId, currentTrackFileName = "";
        private bool currentlyRequesting = false;

        public override void Initialize()
        {
            hook = new MessageHook();
            Player.ServiceMessageDispatcher.Hook(hook);
            try
            {
                config = DeserializeXMLFileToObject<Configuration>(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.xml");
                intervalBetweenRequests = config.IntervalBetweenRequests < 5000 ? 5000 : config.IntervalBetweenRequests;
                if (config.AuthToken == "") { MessageBox.Show("No authentification token for RYM Set Listening set. Check your config.xml."); throw new Exception(); } else { authToken = Uri.UnescapeDataString(config.AuthToken); }
                SetHook();
            }
            catch{}
        }

        private void SetHook()
        {
            hook.OnCoreMessage += (message, param1, param2) =>
            {
                if (message == AimpCoreMessageType.EventPlayableFileInfo)
                {
                    PauseAndExecuter.AbortLast();
                    PauseAndExecuter.Execute(() =>
                    {
                        try
                        {
                            if (!currentlyRequesting && currentTrackFileName != Player.CurrentFileInfo.FileName)
                            {
                                currentlyRequesting = true;
                                currentTrackFileName = Player.CurrentFileInfo.FileName;
                                if (Player.CurrentFileInfo.Album != currentAlbum && Player.CurrentFileInfo.Album != "")
                                {
                                    currentAlbumRymId = GetAlbumID(Player.CurrentFileInfo.Artist.ToLower(), Player.CurrentFileInfo.Album.ToLower());
                                    currentAlbum = Player.CurrentFileInfo.Album;
                                    // Trying to avoid getting sniped by RYM Server, as it doesn't like quick successive requests.
                                    Thread.Sleep(3000);
                                    SetListeningTo(currentAlbumRymId);
                                }
                                else if (Player.CurrentFileInfo.Album == "")
                                {
                                    SetListeningTo(comment: Player.CurrentFileInfo.Title);
                                }
                                currentlyRequesting = false;
                            }
                        }
                        catch { }
                    }
                    , intervalBetweenRequests);
                }
                return ActionResultType.OK;
            };
        }

        public override void Dispose()
        {
            Player.ServiceMessageDispatcher.Unhook(hook);
        }

        public string GetAlbumID(string albumArtist, string albumTitle)
        {
            string albumId = "";
            var request = (HttpWebRequest)WebRequest.Create("https://rateyourmusic.com/search?searchtype=l&searchterm=" + Uri.EscapeDataString(albumArtist + " " + albumTitle));
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            var response = request.GetResponse();
            string pageContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            string albumStringToFind = "title=\"[Album";
            int index = pageContent.IndexOf(albumStringToFind) + albumStringToFind.Length;
            if (index != -1)
            {
                while (pageContent.Length > index && pageContent[index] != ']')
                {
                    albumId += pageContent[index++];
                }
            }
            return albumId;
        }

        public string SetListeningTo(string albumId = "", string comment = "")
        {
            string postData = "";
            var values = new Dictionary<string, string>
                {
                    { "rym_ajax_req", "1" },
                    { "request_token", authToken }
                };
            if (albumId != "")
            {
                values.Add("object", "release");
                values.Add("assoc_id", albumId);
                values.Add("action", "AddPlayHistory");
            }
            else
            {
                values.Add("play_str", comment);
                values.Add("action", "AddPlayHistoryFromText");
            }
            foreach (string key in values.Keys)
            {
                postData += Uri.EscapeDataString(key) + "="
                      + Uri.EscapeDataString(values[key]) + "&";
            }
            byte[] data = Encoding.ASCII.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://rateyourmusic.com/httprequest");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Cookie", $"ulv={authToken};");

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
            var response = request.GetResponse();
            var pageContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return pageContent;
        }
        public static T DeserializeXMLFileToObject<T>(string XmlFilename)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(XmlFilename)) return default(T);
            StreamReader xmlStream = new StreamReader(XmlFilename);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            returnObject = (T)serializer.Deserialize(xmlStream);

            return returnObject;
        }
    }
}
