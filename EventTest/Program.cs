using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace EventTest
{
    // デリゲートとは処理を他に投げ込む場合に間に入るのりのようなもの
    // という理解
    delegate void KeyboadEventHandler(char eventCode);

    /// <summary>
    /// キーボードからの入力イベント待ち受けクラス
    /// </summary>
    class KeyboardEventLoop
    {
        public event KeyboadEventHandler OnKeyDown;
        //public KeyboadEventHandler OnKeyDown;

        public KeyboardEventLoop() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onKeyDown">イベントハンドラ？</param>
        public KeyboardEventLoop(KeyboadEventHandler onKeyDown)
        {
            OnKeyDown += onKeyDown;
        }

        public Task Start(CancellationToken ct)
        {
            return Task.Run(() => EventLoop(ct));
        }

        void EventLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                string line = Console.ReadLine();
                char eventCode = (line == null || line.Length == 0) ? '\0' : line[0];

                // イベント処理はデリゲートを通して他のメソッドに任せる
                OnKeyDown(eventCode);
            }
        }

    }

    class Program
    {
        const string FULL = "yyyy/dd/MM hh:mm:ss\n";
        const string DATE = "yyyy/dd/M\n";
        const string TIME = "hh:mm:ss\n";

        static bool isSuspended = true;
        static string timeFormat = TIME;

        static void Main(string[] args)
        {
            WriteHelp();

            var cts = new CancellationTokenSource();
            //                                    ラムダ式　ctsはクロージャで引き込んでる？
            var eventLoop = new KeyboardEventLoop(code => OnKeyDown(code, cts));

            Task.WhenAll(
                eventLoop.Start(cts.Token),
                TimerLoop(cts.Token)
                ).Wait();
        }

        private static async Task TimerLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (!isSuspended)
                {
                    Console.Write(DateTime.Now.ToString(timeFormat));
                }
                await Task.Delay(1000);
            }
        }

        static void OnKeyDown(char eventCode, CancellationTokenSource cts)
        {
            switch (eventCode)
            {
                case 'r':
                    isSuspended = false;
                    break;
                case 's':
                    isSuspended = true;
                    break;
                case 'f':
                    timeFormat = FULL;
                    break;
                case 'd':
                    timeFormat = DATE;
                    break;
                case 't':
                    timeFormat = TIME;
                    break;
                case 'q':
                    cts.Cancel();
                    break;
                default:
                    WriteHelp();
                    break;
            }
        }

        private static void WriteHelp()
        {
            Console.Write(
                 "使い方\n" +
                 "r (run)    : 時刻表示を開始します。\n" +
                 "s (suspend): 時刻表示を一時停止します。\n" +
                 "f (full)   : 時刻の表示形式を“日付＋時刻”にします。\n" +
                 "d (date)   : 時刻の表示形式を“日付のみ”にします。\n" +
                 "t (time)   : 時刻の表示形式を“時刻のみ”にします。\n" +
                 "q (quit)   : プログラムを終了します。\n"
                 );
        }
    }
}
