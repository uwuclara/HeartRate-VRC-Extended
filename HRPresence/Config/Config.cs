namespace HRPresence.Objects
{
    public class Config
    {
        public bool   input_bluetooth { get; set; } = false;
        public bool   input_hype_rate { get; set; } = false;
        public bool   input_stromno { get; set; } = false;
        
        public string hyperate_or_stromno_session_id { get; set; } = "";
        public string hyperate_apikey { get; set; } = "";
        
        public double delay_restart_input { get; set; } = 5;
        public double delay_serviceRefresh { get; set; } = 2;
        
        public string discord_rpcid { get; set; } = "1055058124266016808";
        public string discord_game_desc { get; set; } = "My HeartRate <3 is";
        
        public bool   output_discord_rpc { get; set; } = false;
        public bool   output_parameters_osc { get; set; } = false;
        public bool   output_chat_osc { get; set; } = false;
        public bool   output_put_to_file { get; set; } = false;
        
        public string osc_address { get; set; } = "127.0.0.1";
        public int    osc_port { get; set; } = 9000;
        public string osc_chatBox_Message { get; set; } = "HeartRate: %HR% BPM <3";
        public string osc_heartrate_name_int { get; set; } = "HR";
        public string osc_heartrate_name_float { get; set; } = "floatHR";
        public string osc_heartrate_name_ones { get; set; } = "onesHR";
        public string osc_heartrate_name_tens { get; set; } = "tensHR";
        public string osc_heartrate_name_hundreds { get; set; } = "hundredsHR";
    }
}