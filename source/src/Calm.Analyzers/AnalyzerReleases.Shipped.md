## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CALM001 | Usage    | Error    | The [CalmHandler] method must have exactly two parameters: (TMessage, CancellationToken)
CALM002 | Usage    | Error    | The [CalmHandler] method must return Task
CALM003 | Usage    | Error    | The [CalmHandler] method must return Task<TResponse>
CALM004 | Usage    | Error    | The first parameter of the [CalmHandler] method must implement ICalmMessage (ICalmCommand, ICalmQuery, or ICalmEvent)
CALM005 | Usage    | Error    | The return type Task<T> must match the TResponse defined in the message interface
CALM006 | Usage    | Error    | The second parameter of the [CalmHandler] method must be System.Threading.CancellationToken
CALM007 | Usage    | Warning  | Do not use ConfigureAwait(false) on Calm methods
CALM008 | Usage    | Warning  | Do not use ConfigureAwait(false) inside Calm handlers
CALM009 | Usage    | Error    | Invalid [CalmImmediate] usage
CALM010 | Usage    | Error    | Invalid [CalmSuppressLog] usage
