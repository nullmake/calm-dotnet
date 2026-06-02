# Architecture & Concepts

This section explains the concurrent design patterns and technical implementation behind CALM.

## 1. Channel-based Single-Threaded Message Pump

`CalmPump` is built on `System.Threading.Channels` using a `BoundedChannel` (SingleReader/MultiWriter).
The core synchronous loop operating on a dedicated thread ("Calm Thread") is designed to smooth out "occupancy time/processing queue characteristics," represented by the following formula:

$$U_{\text{pump}} = \frac{\sum T_{\text{processing}}}{T_{\text{total}}}$$

Here, $U_{\text{pump}}$ represents the message pump's occupancy rate, which can be calculated based on the `calm.engine.processing_duration` histogram from Telemetry.

### Visualizing Message Flow

```text
[ External Threads ]       [ CALM Dedicated Thread (CalmPump) ]
      |                      |
      |-- ExecuteAsync() --->| (Enqueued)
      |                      |
      |                      |---[ Retrieve Next Task ]
      |                      |           |
      |                      |---[ Set SynchronizationContext ]
      |                      |           |
      |                      |---[ Execute Handler (User Code) ]
      |                      |           |
      |                      |---[ Clear SynchronizationContext ]
      |                      |
      |<-- (Completion) -----|
```

### Summary for Beginners
CALM's architecture is like a **"Reception Counter (Queue)"** with a **"Dedicated Attendant (Dedicated Thread)."**
No matter how many threads send requests, the dedicated attendant processes them one by one, sequentially and at high speed. Therefore, there is no risk of the attendant's data being modified by others without permission.

## 2. Thread Affinity and SynchronizationContext

When CalmPump starts, a `CalmSynchronizationContext` is bound to the dedicated thread. This ensures that resumption points of asynchronous operations (continuation tasks) are automatically posted back to the CalmPump's execution queue, guaranteeing they always run on the same thread while maintaining order.

## 3. Atomic Outbox Execution Lifecycle

`CalmBusCore` creates a `CalmExecutionContextState` (Unit of Work context) when command processing begins.

1. The command handler is executed.
2. `Publish` events issued within the handler are pooled in an in-memory Outbox queue.
3. `ExecuteOutboxAsync` is triggered only if the handler completes successfully, executing all pooled event handlers sequentially.
4. If an exception occurs, the Outbox queue is cleared (discarded), providing atomic rollback behavior.
