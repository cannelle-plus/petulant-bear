module PetulantBear.GamesSaga 
//
//
////state machine
////                  Scheduled  -> GameStarted <-> StartDateChanged
////                                            <-> LocationChanged
////                                            <-> MaxPlayerChanged
////                                            -> GameCancelled -> GameClosed !
////                                            -> GameCancelled -> Closed !
//open System
//
//type GameSaga = 
//    {
//        Id: Guid ;
//        MaxPlayers : int;
//        Location :string;
//        StartDate :DateTime;
//    }
//
//type GameStarted = GameSaga
//type GameClosed = GameSaga 
//
////set of states
//type T = 
//    | GameStarted of GameStarted
//    | GameClosed of GameClosed
//
//type Events =
//    |
//
//let changeLocation state location = { state with Location=location;}
//let changeMaxPlayer state maxPlayers = { state with MaxPlayers=maxPlayers;}
//let changeStartDate state startDate = { state with StartDate=startDate;}
//        
//
//let create id location maxplayers startDate = 
//    GameStarted({ Id = id;
//        Location = location;
//        StartDate = startDate;
//        MaxPlayers= maxplayers;
//    })
