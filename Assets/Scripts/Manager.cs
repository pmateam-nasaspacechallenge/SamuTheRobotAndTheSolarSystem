using FullSerializer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Services.Assistant.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.DataTypes;

public class Manager : MonoBehaviour {

	public InputField Speech2Text_InputField;

	public InputField Text2Speech_InputField;

	public Text Assistant_Intent;
    
	// Speech to Text -----------------------------------------------------

    [Header("Speech to Text")]
	[SerializeField]
    private string Speech2Text_ServiceUrl;
    [SerializeField]
    private string Speech2Text_Username;
    [SerializeField]
    private string Speech2Text_Password;

    private Credentials _Speech2Text_Credentials;
    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private SpeechToText Speech2Text_Service;

	// Text to Speech -------------------------------------------------------

    [Header("Text to Speech")]
	[SerializeField]
    private string Text2Speech_ServiceUrl;
    [SerializeField]
    private string Text2Speech_Username;
    [SerializeField]
    private string Text2Speech_Password;

	private Credentials _Text2Speech_Credentials;
	private TextToSpeech Text2Speech_Service;

	// Assistant ------------------------------------------------------------

    [Header("Assistant")]
	[SerializeField]
    private string Assistant_ServiceUrl;
	[SerializeField]
    private string Assistant_Workspace;
	// "The version date with which you would like to use the service in the form YYYY-MM-DD."
	[SerializeField]
    private string Assistant_VersionDate;
    [SerializeField]
    private string Assistant_Username;
    [SerializeField]
    private string Assistant_Password;

    private fsSerializer _Assistant_Serializer = new fsSerializer();
	private Credentials _Assistant_Credentials;
	private Assistant _Assistant_Service;
    private Dictionary<string, object> _Assistant_Context = null;

    // --------------------------------------------------------------------------------------------------------------------------------------------
	
	void Start () {

        LogSystem.InstallDefaultReactors();

        if (!string.IsNullOrEmpty(Speech2Text_Username) && !string.IsNullOrEmpty(Speech2Text_Password))
        {
            _Speech2Text_Credentials = new Credentials(Speech2Text_Username, Speech2Text_Password, Speech2Text_ServiceUrl);
        }

        if (!string.IsNullOrEmpty(Text2Speech_Username) && !string.IsNullOrEmpty(Text2Speech_Password))
        {
            _Text2Speech_Credentials = new Credentials(Text2Speech_Username, Text2Speech_Password, Text2Speech_ServiceUrl);
        }

        if (!string.IsNullOrEmpty(Assistant_Username) && !string.IsNullOrEmpty(Assistant_Password))
        {
            _Assistant_Credentials = new Credentials(Assistant_Username, Assistant_Password, Assistant_ServiceUrl);
        }

	
		StartSpeechToText();
		StartTextToSpeech();
		StartAssistant();

		//Assistant_Message("zoom in");
	}

    void OnDestroy()
    {
        Active = false;
    }

	//
	// Flow ---------------------------------------------------------------------------------------------------------------------------------------
	//

	public void Flow_TextHasArrived(string pureText, string infoText, bool isFinal) 
	{        
        if (null != Speech2Text_InputField) {
		    Speech2Text_InputField.text = infoText;
        }

		/** 
		Text2Speech_InputField.text = pureText;
		*/

		if (isFinal) {
			Assistant_Message(pureText);
		}	
				
	}
   
	public void Flow_IntentArrived(string intent, string response) 
	{
        switch (intent) {
            case "Samu_ZoomIn" : 
            {
                UIActions.Instance.ZoomIn();
                Say(response);
            }
            break;
            
            case "Samu_ZoomOut" : 
            {
                UIActions.Instance.ZoomOut();
                Say(response);
            }
            break;

            case "Samu_TellMeMore" : 
            {
                UIActions.Instance.ToggleFact();
                string fact = UIActions.Instance.GetFactString();
                if (null != fact && fact.Length > 0) {
                    Say(fact);
                } 

            }
            break;

            case "Samu_SpinObject" : 
            {
                UIActions.Instance.ToggleRotation();
                Say(response);
            }
            break;

            default:
            {
                //Say(response);
            }
            break;
        }

		if (Assistant_Intent != null) 
		{
			Assistant_Intent.text = intent; // + " " + response;
		}
	}

	//
	// Assistant ----------------------------------------------------------------------------------------------------------------------------------
	//

	public void StartAssistant() 
	{
        _Assistant_Service = new Assistant(_Assistant_Credentials);
        _Assistant_Service.VersionDate = Assistant_VersionDate;

	}

    public void Assistant_Message(string message) 
	{
 		Dictionary<string, object> input = new Dictionary<string, object>();
        input.Add("text", message);
        MessageRequest messageRequest = new MessageRequest()
        {
            Input = input
        };

        _Assistant_Service.Message(OnAssistantMessage, OnAssistantFail, Assistant_Workspace, messageRequest);
	}

 	private void OnAssistantMessage(object response, Dictionary<string, object> customData)
    {
        Log.Debug("OnMessage()", "Response: {0}", customData["json"].ToString());

        //  Convert resp to fsdata
        fsData fsdata = null;
        fsResult r = _Assistant_Serializer.TrySerialize(response.GetType(), response, out fsdata);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Convert fsdata to MessageResponse
        MessageResponse messageResponse = new MessageResponse();
        object obj = messageResponse;
        r = _Assistant_Serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Set context for next round of messaging
        object _tempContext = null;
        (response as Dictionary<string, object>).TryGetValue("context", out _tempContext);

        if (_tempContext != null)
            _Assistant_Context = _tempContext as Dictionary<string, object>;
        else
            Log.Debug("ExampleAssistantV1.OnMessage()", "Failed to get context");

		// Get output
        object tempOutputObj = null;
        (response as Dictionary<string, object>).TryGetValue("output", out tempOutputObj);
        object tempOutputGenericObj = null;
        (tempOutputObj as Dictionary<string, object>).TryGetValue("generic", out tempOutputGenericObj);
        object tempOutputResponseObj = (tempOutputGenericObj as List<object>)[0];
		object tempRespText;
		(tempOutputResponseObj as Dictionary<string, object>).TryGetValue("text", out tempRespText);
		string responseText = tempRespText.ToString();

        //  Get intent
        object tempIntentsObj = null;
        (response as Dictionary<string, object>).TryGetValue("intents", out tempIntentsObj);
        object tempIntentObj = (tempIntentsObj as List<object>)[0];
        object tempIntent = null;
        (tempIntentObj as Dictionary<string, object>).TryGetValue("intent", out tempIntent);
        string intent = tempIntent.ToString();

        Log.Debug("ExampleAssistantV1.OnMessage()", "intent: {0}", intent);

		Flow_IntentArrived(intent, responseText);		
    }

 	private void OnAssistantFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Debug("ExampleAssistantV1.OnFail()", "Response: {0}", customData["json"].ToString());
        Log.Error("ExampleAssistantV1.OnFail()", "Error received: {0}", error.ToString());
    }

	//
	// Text to Speech -----------------------------------------------------------------------------------------------------------------------------
	//

    #region Text2Speech

	public void StartTextToSpeech() 
	{
		if (_Text2Speech_Credentials != null) {	

			Text2Speech_Service = new TextToSpeech(_Text2Speech_Credentials);

			if (null != Text2Speech_InputField) {

				Text2Speech_InputField.text = "Write what you want to hear...";

				//
				//
				//

				/**
				Text2Speech_InputField.onEndEdit.AddListener(delegate { 
					
					if (Text2Speech_InputField.text.Length > 0) {
						Say(Text2Speech_InputField.text);
					}
				});
				**/
			}
		}
	}


  	public void Say(string text) 
	{
        Text2Speech_Service.Voice = VoiceType.en_US_Michael;
        Text2Speech_Service.ToSpeech(HandleToSpeechCallback, OnFail, text, true);
    }

    private void HandleToSpeechCallback(AudioClip clip, Dictionary<string, object> customData = null)
    {
        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();
            Destroy(audioObject, clip.length);
        }
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleTextToSpeech.OnFail()", "Error received: {0}", error.ToString());
    }

    #endregion

	//
	// Speech to Text -----------------------------------------------------------------------------------------------------------------------------
	//
	
	#region Speech2Text

	public void StartSpeechToText() {
		if (_Speech2Text_Credentials != null) {

			Speech2Text_Service = new SpeechToText(_Speech2Text_Credentials);
			Speech2Text_Service.StreamMultipart = true;
			Active = true;        

            if (null != Speech2Text_InputField) {
			    Speech2Text_InputField.text = "Dont be shy, talk to me!";
            }
			StartRecording();		
		}
	}

    public bool Active
    {
        get { return Speech2Text_Service.IsListening; }
        set
        {
            if (value && !Speech2Text_Service.IsListening)
            {
                Speech2Text_Service.DetectSilence = true;
                Speech2Text_Service.EnableWordConfidence = true;
                Speech2Text_Service.EnableTimestamps = true;
                Speech2Text_Service.SilenceThreshold = 0.01f;
                Speech2Text_Service.MaxAlternatives = 0;
                Speech2Text_Service.EnableInterimResults = true;
                Speech2Text_Service.OnError = S2TOnError;
                Speech2Text_Service.InactivityTimeout = -1;
                Speech2Text_Service.ProfanityFilter = false;
                Speech2Text_Service.SmartFormatting = true;
                Speech2Text_Service.SpeakerLabels = false;
                Speech2Text_Service.WordAlternativesThreshold = null;
                Speech2Text_Service.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && Speech2Text_Service.IsListening)
            {
                Speech2Text_Service.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void S2TOnError(string error)
    {
        Active = false;
        Log.Debug("ExampleStreaming.S2TOnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                Speech2Text_Service.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
					string pureText = alt.transcript;
					string infoText = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);

                    Log.Debug("Manager.OnRecognize()", infoText);

                    //
                    // When the string gets back, this is what happens...
                    //

					Flow_TextHasArrived(pureText, infoText, res.final);
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach(var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }	

	#endregion
}
