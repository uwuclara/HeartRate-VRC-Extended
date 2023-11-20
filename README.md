# HeartRate-VRC-Extended
Is another HeartRate app for VRChat. Gives bunch of config options. Supports Bluetooth(tested), HypeRate(tested) and Stromno(not tested). 
When using HypeRate/Stromno, it uses 200Tigersbloxed's proxy with his API Key. U can try using ur own API Key for HypeRate(not tested).
Recommend using 200Tigersbloxed's repo instead if u don't need extra options.

## Config

```toml
# config.toml

# Services - Select (Must pick 1)
input_bluetooth = false
input_hype_rate = false
input_stromno = false

# Services - Config
hyperate_or_stromno_session_id = "" //For HypeRate u can use test sessionID "internal-testing". Use only sessionID not the whole like (Example) https://app.hyperate.io/internal-testing - use "internal-testing"
hyperate_apikey = "" // todo

# Timeout
delay_restart_input = 5.0 // when heartrate not detected auto reconnect delay
delay_serviceRefresh = 2 //define how often outputs will be updated. OSC parmeters & putToFile do not have delay

# Outputs
output_discord_rpc = false
output_parameters_osc = false
output_chat_osc = false
output_put_to_file = false

# Outputs - Config - Discord
discord_rpcid = "1055058124266016808"
discord_game_desc = "My HeartRate <3 is"

# Outputs - Config - OSC - Remote
osc_address = "127.0.0.1" //When using Quest put IP of ur quest
osc_port = 9000

# Outputs - Config - OSC - ChatBox
osc_chatBox_Message = "HeartRate: %HR% BPM <3" //Must contain %HR%. %HR% gets replaced with ur actual HeartRate

# Outputs - Config - OSC - Avatar Parameters
osc_heartrate_name_int = "HR"
osc_heartrate_name_float = "floatHR"
osc_heartrate_name_ones = "onesHR"
osc_heartrate_name_tens = "tensHR"
osc_heartrate_name_hundreds = "hundredsHR"

```

## OSC Parameters

| Parameter   | Path                           | Description            |
|-------------|--------------------------------|------------------------|
| `HR`        | `/avatar/parameters/HR`        | actual heartrate as int|
| `onesHR`    | `/avatar/parameters/onesHR`    | ones digit             |
| `tensHR`    | `/avatar/parameters/tensHR`    | tens digit             |
| `hundredsHR`|`/avatar/parameters/hundredsHR` | hundreds digit         |
| `floatHR`   | `/avatar/parameters/floatHR`   | maps 0:255 to -1.0:1.0 |


## Repos used to make this happen
https://github.com/Naraenda/HRPresence
https://github.com/200Tigersbloxed/HRtoVRChat_OSC
