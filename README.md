# HeartRate-VRC-Extended
Is another HeartRate app for VRChat. Gives bunch of config options. Supports Bluetooth(tested), HypeRate(tested) and Stromno(not tested). 
When using HypeRate/Stromno, it uses 200Tigersbloxed's proxy with his API Key. 
Recommend using 200Tigersbloxed's repo instead if u don't need extra options.

## Config

```toml
# config.toml

# Services (Must pick 1)
use_bluetooth = false
use_hype_rate = false
use_stromno = false

# Services - Config
hype_rate_or_stromno_session_id = "" //For HypeRate u can use test sessionID "internal-testing". Use only sessionID not the whole like (Example) https://app.hyperate.io/internal-testing - use "internal-testing"
hype_rate_apikey = ""

# Timeout
time_out_interval = 4.0
restart_delay = 4.0

# Outputs
use_discord_rpc = false
use_parameters_osc = false
use_chat_osc = false
put_to_file = false

# Outputs - Config
discord_rpcid = "1055058124266016808"
discord_game_desc = "My HeartRate <3 is"
chat_oscmessage = "HeartRate: %HR% BPM <3"

# Outputs - Config - OSC Remote
oscaddress = "127.0.0.1" //When using Quest put IP of ur quest
oscport = 9000

# Outputs - Config - OSC Avatar Parameters
oscheart_rate_name_int = "HR"
oscheart_rate_name_float = "floatHR"
oscheart_rate_name_ones = "onesHR"
oscheart_rate_name_tens = "tensHR"
oscheart_rate_name_hundreds = "hundredsHR"

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