﻿namespace Saithis.MessageBus;

public interface IMessageSerializer
{
    byte[] Serialize(object message, MessageProperties messageProperties);
}