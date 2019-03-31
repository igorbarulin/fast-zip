using System;
using System.Linq;
using System.Threading;
using MultiThreadGzip.Components;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;
using MultiThreadGzip.Processors;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    [Category("Unit/ProcessorQueueTests")]
    public class ProcessorQueueTests
    {
        private volatile int _completedTasks;
        private ITaskHandler _defaultTaskHandler;

        [SetUp]
        public void SetUp()
        {
            _completedTasks = 0;
            _defaultTaskHandler = new TaskHandler<object>(o =>
            {
                Thread.Sleep(200);
                Interlocked.Increment(ref _completedTasks);
            });
        }
        
        public enum QueueType
        {
            Common,
            Splitter
        }
        
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestCreateBasicFlow(QueueType type)
        {
            Assert.DoesNotThrow(() => CreateProcessorQueue(type, _defaultTaskHandler, null, exception => { }, 1));
            Assert.DoesNotThrow(() => CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1));
        }
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestCreateTaskHandlerNull(QueueType type)
        {
            Assert.Throws<ArgumentNullException>(() => CreateProcessorQueue(type, null, null, null, 1));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestCreateQueueBufferOutOfRange(QueueType type)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProcessorQueue(type, _defaultTaskHandler, null, null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProcessorQueue(type, _defaultTaskHandler, null, null, -1));
        }
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestTryEnqueueBufferFree(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.That(queue.TryEnqueue(new object()), Is.True);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(1));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestTryEnqueueBufferFull(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.That(queue.TryEnqueue(new object()), Is.True);
                
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.False);
                Assert.That(queue.TryEnqueue(new object()), Is.False);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(1));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestTryEnqueueNullTask(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.That(queue.TryEnqueue(null), Is.False);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(0));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestTryEnqueueQueueWasCancelled(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                
                queue.Cancel();
                
                Assert.That(queue.IsRunning, Is.False);
                Assert.That(queue.ReadyToEnqueue, Is.False);
                Assert.That(queue.TryEnqueue(new object()), Is.False);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(0));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestTryEnqueueInnerQueueWasCancelled(QueueType type)
        {
            using (var queue2 = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            using (var queue1 = CreateProcessorQueue(type, _defaultTaskHandler, queue2, null, 1))
            {
                Assert.That(queue2.IsRunning, Is.True);
                Assert.That(queue2.ReadyToEnqueue, Is.True);
                
                queue2.Cancel();
                
                Assert.That(queue2.IsRunning, Is.False);
                Assert.That(queue2.ReadyToEnqueue, Is.False);
                
                Assert.That(queue1.IsRunning, Is.True);
                Assert.That(queue1.ReadyToEnqueue, Is.True);
                Assert.That(queue1.TryEnqueue(new object()), Is.True);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(1));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestWaitWhenQueueWasCancelled(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                queue.Cancel();
                
                Assert.That(queue.IsRunning, Is.False);
                Assert.That(queue.ReadyToEnqueue, Is.False);
                Assert.Throws<OperationCanceledException>(() => queue.WaitForReady());
            }
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        [Timeout(200)]
        public void TestWaitWhileSleeping(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Thread.Sleep(100);
                queue.WaitForReady();
            }
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestEnqueueBufferFree(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.DoesNotThrow(() => queue.Enqueue(new object()));
            }
            
            Assert.That(_completedTasks, Is.EqualTo(1));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestEnqueueBufferFull(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.DoesNotThrow(() => queue.Enqueue(new object()));
                
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.False);
                Assert.DoesNotThrow(() => queue.Enqueue(new object()));
            }
            
            Assert.That(_completedTasks, Is.EqualTo(2));
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestEnqueueNullTask(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                Assert.Throws<NullReferenceException>(() => queue.Enqueue(null));
            }
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestEnqueueQueueWasCancelled(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            {
                Assert.That(queue.IsRunning, Is.True);
                Assert.That(queue.ReadyToEnqueue, Is.True);
                
                queue.Cancel();
                
                Assert.That(queue.IsRunning, Is.False);
                Assert.That(queue.ReadyToEnqueue, Is.False);
                Assert.Throws<OperationCanceledException>(() => queue.Enqueue(new object()));
            }
        }

        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestEnqueueInnerQueueWasCancelled(QueueType type)
        {
            using (var queue2 = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 1))
            using (var queue1 = CreateProcessorQueue(type, _defaultTaskHandler, queue2, null, 1))
            {
                Assert.That(queue2.IsRunning, Is.True);
                Assert.That(queue2.ReadyToEnqueue, Is.True);
                
                queue2.Cancel();
                
                Assert.That(queue2.IsRunning, Is.False);
                Assert.That(queue2.ReadyToEnqueue, Is.False);
                
                Assert.That(queue1.IsRunning, Is.True);
                Assert.That(queue1.ReadyToEnqueue, Is.True);
                queue1.Enqueue(new object());
            }
            
            Assert.That(_completedTasks, Is.EqualTo(1));
        }
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestExceptionIgnored(QueueType type)
        {
            var taskHandler = new TaskHandler<object>(o =>
            {
                _completedTasks++;
                throw new NullReferenceException();
            });
            
            using (var queue = CreateProcessorQueue(type, taskHandler, null, null, 1))
            {
                queue.Enqueue(new object());
                
                queue.WaitForReady();
                
                Assert.That(queue.TryEnqueue(new object()), Is.True);
            }
            
            Assert.That(_completedTasks, Is.EqualTo(2));
        }
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestExceptionHandle(QueueType type)
        {
            var taskHandler = new TaskHandler<object>(o =>
            {
                _completedTasks++;
                throw new NullReferenceException();
            });

            var exWasHandled = false;
            var onException = new Action<Exception>(ex =>
            {
                Assert.That(ex is NullReferenceException, Is.True);
                exWasHandled = true;
            });
            
            using (var queue = CreateProcessorQueue(type, taskHandler, null, onException, 1))
            {
                queue.Enqueue(new object());
                
                queue.WaitForReady();
                
                Assert.That(queue.TryEnqueue(new object()), Is.True);
            }
            
            Assert.That(exWasHandled, Is.True);
            Assert.That(_completedTasks, Is.EqualTo(2));
        }
        
        [TestCase(QueueType.Common)]
        [TestCase(QueueType.Splitter)]
        public void TestWaitAllTasksOnDispose(QueueType type)
        {
            using (var queue = CreateProcessorQueue(type, _defaultTaskHandler, null, null, 2))
            {
                Assert.That(queue.TryEnqueue(new object()), Is.True);
                Assert.That(queue.TryEnqueue(new object()), Is.True);
            }

            Assert.That(_completedTasks, Is.EqualTo(2));
        }
        
        private static IProcessorQueue CreateProcessorQueue(QueueType type, ITaskHandler taskHandler,
            IProcessorQueue nextProcessorQueue, Action<Exception> onException, int buffer)
        {
            switch (type)
            {
                case QueueType.Common:
                    return new ProcessorQueue(taskHandler, nextProcessorQueue, onException, buffer);
                case QueueType.Splitter:
                    return CreateProcessorQueueSplitter(1, taskHandler, nextProcessorQueue, onException, buffer);
                default:
                    throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => type), type, null);
            }
        }
        
        private static IProcessorQueue CreateProcessorQueueSplitter(int splitsCount, ITaskHandler taskHandler, IProcessorQueue nextProcessorQueue, Action<Exception> onException, int buffer)
        {
            return new ProcessorQueueSplitter(Enumerable.Range(1, splitsCount).Select(i =>
                (IProcessorQueue) new ProcessorQueue(taskHandler, nextProcessorQueue, onException, buffer)).ToArray());
        }
    }
}