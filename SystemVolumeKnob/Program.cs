using System;
using System.Linq;
using System.Collections.Generic;
using NAudio.CoreAudioApi;



namespace SystemVolumeKnob
{
    class Program
    {
        static void Main(string[] args)
        {
            var volume = 3;
            var parseResult = args.Length > 0 ?int.TryParse(args[0], out volume) : false;

            var knob = new SystemVolumeKnob();
            knob.Initialize();

            knob.Volume = volume;
        }
    }

    /// <summary>
    /// ミキサーのシステムボリューム値を取得/変更する為のクラス
    /// </summary>
    public class SystemVolumeKnob
    {
        #region Properties.
        public int Volume 
        {
            get => (int)Math.Round(GetSystemAudioVolume() * device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            set => SetSystemAudioVolume(Math.Max(0, Math.Min(value*0.01f, 1.0f))); 
        }
        #endregion Properties.

        #region Members.
        private MMDevice device;
        private SimpleAudioVolume systemVolume;
        #endregion Members.

        public SystemVolumeKnob()
        {

        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            var deviceEnumrator = new MMDeviceEnumerator();
            
            device = deviceEnumrator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            List<AudioSessionControl> audioSessions = null;

            ////
            /// AudioSessinのリストを取得
            /// ※ 今回はaudioSessionをあえてリスト化しているが、このリストは本来動的に変わるものだと思われる。
            /// アプリケーションの起動/終了の都度など。 (だからNAudioでもListではなくListっぽく扱えるようにしている、に留めている)
            /// なので静的なリストであってはならないので、この実装は良くないと思われる。
            /// 今回は瞬間的な利用なのでいったん良しとする。
            //
            {
                var tmpSessions = device?.AudioSessionManager.Sessions;

                if (tmpSessions == null)
                {
                    return false;
                }

                audioSessions = new List<AudioSessionControl>();

                for (var i = 0; i < tmpSessions.Count; i++)
                {
                    audioSessions.Add(tmpSessions[i]);
                }
            }

            systemVolume = audioSessions.FirstOrDefault(x => x.IsSystemSoundsSession)?.SimpleAudioVolume;

            if (systemVolume == null)
            {
                return false;
            }

            return true; 
        }

        /// <summary>
        /// ミキサーのシステムボリューム値を取得(0.0～1.0)
        /// </summary>
        /// <returns></returns>
        private float GetSystemAudioVolume ()
        {
            if (systemVolume == null)
            {
                return 0;
            }

            return systemVolume.Volume;
        }

        /// <summary>
        /// ミキサーのシステムボリューム値を設定する(0.0～1.0)
        /// </summary>
        /// <param name="vol">音量</param>
        private void SetSystemAudioVolume(float vol)
        {
            if (systemVolume == null)
            {
                return;
            }

            systemVolume.Volume = vol;
        }
    }
}
