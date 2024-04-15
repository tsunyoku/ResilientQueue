# ResilientQueue

A redis-based queue abstraction made to make async processing simple. Includes configuration & a deadletter queue.

Note that while the queue processes in batches, the order of processing of each item within the batch is not guaranteed - this is intentional.