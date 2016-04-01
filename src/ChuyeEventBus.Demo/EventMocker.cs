using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {
    public static class EventMocker {
        public static void MockClient() {
            for (int i = 0; i < 6; i++) {
                MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = i + 1 });
            }
            for (int i = 0; i < 8; i++) {
                MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = 60 + i * 10 });
            }
        }

        public static void MockClientAsync() {
            var random = new Random();
            var works = Enumerable.Range(0, 100).Select(r => random.Next(1, 100)).ToArray();
            
            Task.Run(action: () => {
                while (true) {
                    Console.WriteLine("Send FansFollowEvent");
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 1000 + 1000));
                }
            });

            Task.Run(action: () => {
                var index = 0;
                while (true) {
                    Console.WriteLine("Send WorkPublishEvent");
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = ++index });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode()) % 200 + 300);
                }
            });
        }
    }
}
