
# Setup

## Rabbit independent

* [Message("id")]
   * used to know id for publish
   * used to find type for receive

## Rabbit specific

* Manually create exchanges, queues and bindings
  * with rabbitmq client directly
  * full control
  * explicit
* Publish<Message>("exchange", "routeKey")
  * Builds lookup dict for Sender to know the exchange
* Listen("queue", x => x.Add<Message>().Add<Message2>())
  * Adds rabbit queue listener
  * Registers IMessageHandler<Message>'s
    * queue listener gets message
    * finds type by id from Add<Message>() types
    * finds the handler from type
    * calls it

# AsyncApi

* Get Publish<Message>("exchange", "routeKey") config 
   * for sending info
* Get Listen("queue", x => x.Add<Message>().Add<Message2>()) config 
   * for receive info

