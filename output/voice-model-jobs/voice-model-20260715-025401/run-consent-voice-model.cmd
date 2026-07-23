@echo off
rem Consent-based voice model runbook.
rem Job: C:\MyProject\PoliticalLeaderPortal\output\voice-model-jobs\voice-model-20260715-025401\voice-model-20260715-025401.json
rem Voice sample: C:\Users\Admin\Downloads\Atul Pradhan attacks Sangeet Som भड़के अतुल प्रधान ने संगीत सोम के धागे खोल डाले! - UP Tak.mp3
echo Review consent metadata before running.
echo Generated output must be labeled AI-generated.
rem Example Coqui XTTS command:
rem xtts --speaker_wav "C:\Users\Admin\Downloads\Atul Pradhan attacks Sangeet Som भड़के अतुल प्रधान ने संगीत सोम के धागे खोल डाले! - UP Tak.mp3" --text "Create a consent-based Hindi voiceover sample for a public information video." --out_path "C:\MyProject\PoliticalLeaderPortal\output\voice-model-jobs\voice-model-20260715-025401\renders\voice-model-20260715-025401-render.wav"
echo Configure provider, then uncomment and run the command above.
