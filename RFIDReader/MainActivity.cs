using Android.App;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Content;
using System;

namespace RFIDReader
{
    [Activity(Label = "RFIDReader", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView LogMessage;

        private NfcAdapter _adapter = null;
        private NfcAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    _adapter = NfcAdapter.GetDefaultAdapter(this);
                }
                return _adapter;
            }
        }

        private void WriteLog(string str)
        {
            LogMessage.Text = str;
        }

        bool _inWriteMode;

        protected override void OnResume()
        {
            base.OnResume();
            EnableWriteMode();
        }

        protected override void OnPause()
        {
            base.OnPause();
            // App is paused, so no need to keep an eye out for NFC tags.
            if (_adapter != null) _adapter.DisableForegroundDispatch(this);
        }

        // <summary>
        /// Identify to Android that this activity wants to be notified when 
        /// an NFC tag is discovered. 
        /// </summary>
        private void EnableWriteMode()
        {
            _inWriteMode = true;

            // Create an intent filter for when an NFC tag is discovered.  When
            // the NFC tag is discovered, Android will 
            IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
            var filters = new[] { tagDetected };

            // When an NFC tag is detected, Android will use the PendingIntent to come back to this activity.
            // The OnNewIntent method will invoked by Android.
            var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
            try
            {
                Adapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                throw;
            }
        }

        static string bin2hex(byte[] data)
        {
            string hex = BitConverter.ToString(data);
            return hex.Replace("-","");
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (_inWriteMode)
            {
                _inWriteMode = false;
                Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

                if (tag == null)
                {
                    return;
                }

                byte[] Uid = tag.GetId();
                string UidString = bin2hex(Uid);
                WriteLog(UidString);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            LogMessage = FindViewById<TextView>(Resource.Id.mifare_log);
            WriteLog("Screen started");
            if (Adapter == null)
            {
                WriteLog("No NFC detected");
            }
            else
            {
                WriteLog("Waiting for Card");
            }
        }
    }
}

