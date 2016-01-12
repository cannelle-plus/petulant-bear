[<AutoOpen>]
module types

open System

type MailNotification = 
    {
        NotificationId:Guid;
        Recipient:string;
        Subject :string;
        Body :string;
        ScheduledDate :DateTime;
        NbAttempt : int;
    } 

