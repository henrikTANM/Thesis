using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Message;

public class Message
{
    public string message;
    public MessageType messageType;
    public MessageSender sender;
    public SenderType senderType;

    public enum MessageType
    {
        NOTIFICATION,
        WARNING
    }

    public enum SenderType
    {
        STAR,
        PLANET,
        PRODUCTIONBUILDING,
        DISCOVERYHUB,
        BHCF,
        ROUTE,
        SYSTEM
    }

    public Message(string message, MessageType messageType, MessageSender sender, SenderType senderType)
    {
        this.message = message;
        this.messageType = messageType;
        this.sender = sender;
        this.senderType = senderType;
    }
}

public class MessageSender {

}

public class MessageSender<T> : MessageSender
{
    public T sender;
    public MessageSender(T sender) { this.sender = sender; }
}
