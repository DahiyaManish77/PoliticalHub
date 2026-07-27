using PoliticalLeaderPortal.Services;
using System;
using System.Security;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class VoiceAgentWebhookController : Controller
    {
        private readonly VoiceAgentService _service = new VoiceAgentService();
        private readonly BulkVoiceCallerService _bulkService = new BulkVoiceCallerService();

        [HttpPost]
        public ActionResult BulkAnswer(string key, int campaignId)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            return Xml(_bulkService.GetAnswerXml(campaignId));
        }

        [HttpPost]
        public ActionResult Incoming(string key)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            var setting = _service.GetSetting();
            if (!setting.IsEnabled) return Xml("<Response><Say>Voice service is currently unavailable.</Say><Hangup/></Response>");

            string callId = Value("CallSid", "call_id", "CallUUID");
            string from = Value("From", "from", "CallFrom");
            string to = Value("To", "to", "DialWhomNumber");
            _service.UpsertCall(callId, "inbound", from, to, "answered", 0);

            string callback = Absolute("Status", key);
            string recording = Absolute("Recording", key);
            string speech = Absolute("HandleSpeech", key) + "&callId=" + Uri.EscapeDataString(callId);
            string recordingStart = setting.EnableRecording
                ? "<Start><Recording recordingStatusCallback=\"" + Escape(recording) + "\" /></Start>"
                : "";
            string xml = "<Response>" + recordingStart +
                "<Say language=\"hi-IN\">" + Escape(setting.RecordingConsentText) + "</Say>" +
                "<Gather input=\"speech dtmf\" language=\"hi-IN\" speechTimeout=\"auto\" timeout=\"5\" numDigits=\"1\" action=\"" + Escape(speech) + "\" method=\"POST\">" +
                "<Say language=\"hi-IN\">" + Escape(setting.WelcomeMessageHindi) + "</Say>" +
                "<Say language=\"en-IN\">" + Escape(setting.WelcomeMessageEnglish) + "</Say>" +
                "<Say language=\"hi-IN\">शिकायत के लिए 1, मिलने के समय के लिए 2, निमंत्रण के लिए 3, कार्यकर्ता बनने के लिए 4, और कार्यालय से बात करने के लिए 0 दबाएँ।</Say></Gather>" +
                "<Redirect method=\"POST\">" + Escape(speech) + "</Redirect>" +
                "</Response>";
            Response.Headers["X-Voice-Status-Callback"] = callback;
            return Xml(xml);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult HandleSpeech(string key, string callId)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            string words = Value("SpeechResult", "speech", "transcript");
            string digits = Value("Digits", "digits");
            if (String.IsNullOrWhiteSpace(words)) words = DigitText(digits);
            double confidence;
            Double.TryParse(Value("Confidence", "confidence"), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out confidence);
            if (String.IsNullOrWhiteSpace(words) || (confidence > 0 && confidence < .45))
            {
                string retry = Absolute("HandleSpeech", key) + "&callId=" + Uri.EscapeDataString(callId ?? "");
                return Xml("<Response><Gather input=\"speech dtmf\" language=\"hi-IN\" speechTimeout=\"auto\" timeout=\"6\" action=\"" +
                    Escape(retry) + "\" method=\"POST\"><Say language=\"hi-IN\">आवाज़ साफ़ नहीं आई। कृपया धीरे-धीरे दोबारा बोलें, या शिकायत के लिए 1 और कार्यालय के लिए 0 दबाएँ।</Say></Gather></Response>");
            }
            string intent = Classify(words);
            _service.AddSpeech(callId, words, "hi-IN", intent);
            string repeat = Absolute("HandleSpeech", key) + "&callId=" + Uri.EscapeDataString(callId ?? "");
            string response = Reply(intent);
            return Xml("<Response><Say language=\"hi-IN\">" + Escape(response) +
                "</Say><Gather input=\"speech dtmf\" language=\"hi-IN\" speechTimeout=\"auto\" action=\"" +
                Escape(repeat) + "\" method=\"POST\"><Say language=\"hi-IN\">क्या आप कुछ और बताना चाहते हैं?</Say></Gather>" +
                "<Say language=\"hi-IN\">धन्यवाद। आपका संदेश जनसंपर्क कार्यालय को भेज दिया गया है।</Say></Response>");
        }

        [HttpPost]
        public ActionResult Status(string key)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            int duration; Int32.TryParse(Value("CallDuration", "Duration", "duration"), out duration);
            string providerCallId = Value("CallSid", "call_id", "CallUUID");
            string providerStatus = Value("CallStatus", "status");
            _service.UpsertCall(providerCallId, Value("Direction", "direction"),
                Value("From", "from", "CallFrom"), Value("To", "to", "DialWhomNumber"),
                providerStatus, duration);
            _bulkService.ProcessStatus(providerCallId, providerStatus);
            return new HttpStatusCodeResult(204);
        }

        [HttpPost]
        public ActionResult Recording(string key)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            int duration; Int32.TryParse(Value("RecordingDuration", "duration"), out duration);
            _service.UpdateRecording(Value("CallSid", "call_id", "CallUUID"), Value("RecordingUrl", "recording_url"), duration);
            return new HttpStatusCodeResult(204);
        }

        [HttpGet]
        public ActionResult Health(string key)
        {
            if (!_service.IsValidSecret(key)) return new HttpStatusCodeResult(403);
            var setting = _service.GetSetting();
            return Json(new { success = true, enabled = setting.IsEnabled, provider = setting.ProviderName,
                numberConfigured = !String.IsNullOrWhiteSpace(setting.PhoneNumber), serverTime = DateTime.UtcNow },
                JsonRequestBehavior.AllowGet);
        }

        private string Value(params string[] names)
        {
            foreach (string name in names)
                if (!String.IsNullOrWhiteSpace(Request[name])) return Request[name];
            return "";
        }
        private string Absolute(string action, string key)
        {
            return Url.Action(action, "VoiceAgentWebhook", new { key = key }, Request.Url == null ? "https" : Request.Url.Scheme);
        }
        private ContentResult Xml(string xml) { return Content(xml, "text/xml"); }
        private static string Escape(string value) { return SecurityElement.Escape(value ?? ""); }
        private static string Classify(string text)
        {
            text = (text ?? "").ToLowerInvariant();
            if (text.Contains("कार्यालय") || text.Contains("staff") || text.Contains("operator") || text.Contains("प्रतिनिधि")) return "Staff callback";
            if (text.Contains("appointment") || text.Contains("मिलना") || text.Contains("समय")) return "Appointment";
            if (text.Contains("शिकायत") || text.Contains("problem") || text.Contains("समस्या")) return "Citizen issue";
            if (text.Contains("निमंत्रण") || text.Contains("invitation")) return "Invitation";
            if (text.Contains("volunteer") || text.Contains("कार्यकर्ता")) return "Volunteer";
            if (text.Contains("सड़क") || text.Contains("नाली") || text.Contains("road") || text.Contains("drain")) return "Roads and drainage";
            if (text.Contains("बिजली") || text.Contains("खंभा") || text.Contains("light")) return "Electricity";
            if (text.Contains("पानी") || text.Contains("नल") || text.Contains("water")) return "Drinking water";
            if (text.Contains("राशन") || text.Contains("ration")) return "Ration";
            if (text.Contains("पेंशन") || text.Contains("pension")) return "Pension";
            if (text.Contains("अस्पताल") || text.Contains("दवाई") || text.Contains("doctor") || text.Contains("health")) return "Healthcare";
            if (text.Contains("किसान") || text.Contains("फसल") || text.Contains("खेत") || text.Contains("farmer")) return "Agriculture";
            if (text.Contains("पुलिस") || text.Contains("थाना") || text.Contains("police")) return "Police assistance";
            if (text.Contains("नौकरी") || text.Contains("रोजगार") || text.Contains("job")) return "Employment";
            return "General enquiry";
        }
        private static string Reply(string intent)
        {
            if (intent == "Staff callback") return "कार्यालय से वापस कॉल करने का अनुरोध दर्ज हो गया है।";
            if (intent == "Appointment") return "आपका अपॉइंटमेंट अनुरोध दर्ज कर लिया गया है।";
            if (intent == "Citizen issue") return "आपकी समस्या दर्ज कर ली गई है। कृपया अपने गाँव, ब्लॉक और समस्या की जगह साफ़-साफ़ बताएँ।";
            if (intent == "Invitation") return "आपका निमंत्रण संदेश दर्ज कर लिया गया है।";
            if (intent == "Volunteer") return "कार्यकर्ता के रूप में जुड़ने की आपकी रुचि दर्ज कर ली गई है।";
            if (intent == "Roads and drainage") return "सड़क या नाली की समस्या दर्ज हुई। कृपया गाँव और सही स्थान का नाम बताएँ।";
            if (intent == "Electricity") return "बिजली की समस्या दर्ज हुई। कृपया गाँव, मोहल्ला और खंभे या ट्रांसफार्मर की जानकारी बताएँ।";
            if (intent == "Drinking water") return "पानी की समस्या दर्ज हुई। कृपया गाँव और खराब नल या पाइपलाइन का स्थान बताएँ।";
            if (intent == "Ration") return "राशन संबंधी समस्या दर्ज हुई। कृपया गाँव और राशन दुकान की जानकारी बताएँ।";
            if (intent == "Pension") return "पेंशन सहायता का अनुरोध दर्ज हुआ। कार्यालय की टीम दस्तावेज़ों के लिए संपर्क करेगी।";
            if (intent == "Healthcare") return "स्वास्थ्य संबंधी अनुरोध दर्ज हुआ। आपात स्थिति में तुरंत एक सौ आठ पर संपर्क करें।";
            if (intent == "Agriculture") return "किसान या फसल संबंधी समस्या दर्ज हुई। कृपया गाँव और समस्या का पूरा विवरण बताएँ।";
            if (intent == "Police assistance") return "पुलिस सहायता अनुरोध दर्ज हुआ। आपात स्थिति में तुरंत एक सौ बारह पर कॉल करें।";
            if (intent == "Employment") return "रोजगार या कौशल प्रशिक्षण का अनुरोध दर्ज हुआ। टीम आगे की जानकारी के लिए संपर्क करेगी।";
            return "आपका संदेश दर्ज कर लिया गया है।";
        }
        private static string DigitText(string digit)
        {
            if (digit == "1") return "शिकायत";
            if (digit == "2") return "appointment";
            if (digit == "3") return "निमंत्रण";
            if (digit == "4") return "कार्यकर्ता";
            if (digit == "0") return "कार्यालय प्रतिनिधि";
            return "";
        }
    }
}
