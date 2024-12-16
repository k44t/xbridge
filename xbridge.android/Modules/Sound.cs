using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Media;
using Android.Net;

namespace xbridge.android.Modules
{


    public class Sound
    {

        public class Player : XBridgeSharedObject
        {
            private int player;
            private SoundPool Pool;
            private float _Volume = 1;
            private bool _Playing = false;

            public Player(Sound sound, string path, string channel) : base(sound.bridge)
            {

                var fd = sound.bridge.GetModule<xbridge.android.Modules.Resources>()._FileDescriptor(path);

                //player.SetDataSource(fd);
                //player = new MediaPlayer();
                if (channel == "alarm")
                {
                    Pool = sound.alarm;
                    //attr.SetUsage(AudioUsageKind.Alarm);
                }
                else if (channel == "notification")
                {
                    Pool = sound.notification;
                }
                else if (channel == "ring")
                {
                    Pool = sound.ring;
                }
                else if (channel == "media" || channel == null)
                {
                    Pool = sound.media;
                }
                else
                {
                    throw new Exception("unknown media channel");
                }
                player = Pool.Load(fd, 1);
            }


            public void Volume(float volume)
            {
                this._Volume = volume;
            }

            public void Play()
            {
                //Console.WriteLine("millis");
                _Playing = true;
                Pool.Play(player, _Volume, _Volume, 999, 0, 1);
            }

            public void Pause()
            {
                _Playing = false;
                Pool.Pause(player);
            }
            public void Stop()
            {
                _Playing = false;
                Pool.Stop(player);
            }

            override public void Destroy()
            {
                Pool.Unload(player);
            }
            public bool Playing()
            {
                return _Playing;
            }
            /*
            public Task<object> Ended(string id)
            {
                TaskCompletionSource<object> src = new TaskCompletionSource<object>();
                EventHandler handler = null;
                player.Completion += handler = (s, e) => {
                    player.Completion -= handler;
                    src.SetResult(null);
                };
                return src.Task;
            }*/
        }

        public Sound(XBridge bridge)
        {
            this.bridge = bridge;
            var poolBuilder = new SoundPool.Builder();
            var attrBuilder = new AudioAttributes.Builder();
            this.alarm = poolBuilder.SetAudioAttributes(attrBuilder.SetUsage(AudioUsageKind.Alarm).Build()).Build();
            this.notification = poolBuilder.SetAudioAttributes(attrBuilder.SetUsage(AudioUsageKind.NotificationEvent).Build()).Build();
            this.ring = poolBuilder.SetAudioAttributes(attrBuilder.SetUsage(AudioUsageKind.NotificationRingtone).Build()).Build();
            this.media = poolBuilder.SetAudioAttributes(attrBuilder.SetUsage(AudioUsageKind.Media).Build()).Build();
        }

        public Player Create(string path, string channel)
        {
            return new Player(this, path, channel);
        }

        private XBridge bridge;
        private SoundPool alarm;
        private SoundPool notification;
        private SoundPool ring;
        private SoundPool media;
        /*
public void DestroyAll()
{
string[] keys = new string[Sounds.Keys.Count];
Sounds.Keys.CopyTo(keys, 0);
foreach(var key in keys)
{
Destroy(key);
}
}*/
    }

}
