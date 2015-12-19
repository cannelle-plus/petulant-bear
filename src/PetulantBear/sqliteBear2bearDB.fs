module PetulantBear.sqliteBear2bearDB

open System
//open Mono.Data.Sqlite
open System.Data.SQLite
open System.Configuration   
open PetulantBear.AfterGames
open PetulantBear.Games
open PetulantBear.Bears
open PetulantBear.Rooms
open PetulantBear.CurrentBear


open PetulantBear.Games.Contracts
open PetulantBear.AfterGames.Contracts
open PetulantBear.Bears.Contracts
open PetulantBear.Rooms.Contracts
open PetulantBear.CurrentBear.Contracts

open System.Linq


let dbConnection = ConfigurationManager.ConnectionStrings.["bear2bearDB"].ConnectionString

//let isMonoRuntime = Type.GetType("Mono.Runtime") != null
//let connection = if (isMonoRuntime) then new SqliteConnection(dbConnection)  else new SQLiteConnection(dbConnection)

let scheduleToDB (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:ScheduleGame) =
    sqlCmd.CommandText <- "Insert into GamesList (id,name,ownerId,ownerBearName,startDate,location,maxPlayers) VALUES (@id, @name,@ownerId,@ownerUserName, @begins, @location, @maxPlayers); Insert into GamesBears (gameId,bearId)  VALUES (@id,@ownerId); Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name);"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@name", cmd.name)
    add("@ownerId", bear.bearId.ToString())
    add("@ownerUserName", bear.username)
    add("@begins", cmd.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    add("@location", cmd.location.ToString())
    add("@maxPlayers", cmd.maxPlayers.ToString())


let joinToDB (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  =
    sqlCmd.CommandText <- "delete from GamesBears  where gameId=@id and bearId=@bearId; Insert into GamesBears (gameId,bearId)  VALUES (@id,@bearId)"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", bear.bearId.ToString())


let cancelToDB (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  =
    sqlCmd.CommandText <- "update GamesList set currentState=0 where id=@id"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())


let abandonToDB (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  =
    sqlCmd.CommandText <- "delete from GamesBears  where gameId=@id and bearId=@bearId"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", bear.bearId.ToString())
    

let changeName (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:Games.Contracts.ChangeName) =
    sqlCmd.CommandText <- "Update  GamesList set name= @name where id=@id"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@name", cmd.name)
    

let changeLocation (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:Games.Contracts.ChangeLocation) =
    sqlCmd.CommandText <- "Update  GamesList set location= @location where id=@id"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@location", cmd.location)

let changeStartDate (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:Games.Contracts.ChangeStartDate) =
    sqlCmd.CommandText <- "Update  GamesList set startDate= @startDate where id=@id"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@startDate", cmd.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))


let changeMaxPlayer (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:Games.Contracts.ChangeMaxPlayer) =
    sqlCmd.CommandText <- "Update  GamesList set maxPlayers= @maxPlayers where id=@id"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@maxPlayers", cmd.maxPlayers.ToString())


let kickPlayer (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (cmd:Games.Contracts.KickPlayer) =
    sqlCmd.CommandText <- "delete from GamesBears  where gameId=@id and bearId=@bearId"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", cmd.kickedBearId.ToString())





let markBearToDB (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession) (cmd:MarkBear)=

    sqlCmd.CommandText <- "Update GamesBears  set mark=@mark where BearId=@bearId and gameId=@gameId;"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", id.ToString())
    add("@bearId", cmd.bearId.ToString())
    add("@mark", cmd.mark.ToString())
    
    


let commentBearToDB (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  (cmd:CommentBear) =

    sqlCmd.CommandText <- "Update GamesBears  set comment=@comment where BearId=@bearId and gameId=@gameId;"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", id.ToString())
    add("@bearId", cmd.bearId.ToString())
    add("@comment", cmd.comment)
    


let getGame bearId (filter:GamesFilter) = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    //retrieving players for the game
    let sqlPlayers = "SELECT        b.bearId, b.bearUsername, b.bearAvatarId, BearsSelection.mark, BearsSelection.comment, GamesList.maxPlayers
                        FROM            Bears b INNER JOIN
                                                 GamesBears BearsSelection ON BearsSelection.bearId = b.bearId INNER JOIN
                                                 GamesList ON GamesList.id = BearsSelection.gameId
                        WHERE        BearsSelection.gameId =@gameId  "
    use sqlCmdPlayers = new SQLiteCommand(sqlPlayers, connection) 

    let add (name:string, value: string) = 
        sqlCmdPlayers.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", filter.id.ToString())


    use readerPlayers = sqlCmdPlayers.ExecuteReader() 
    let bearPlayersList = new System.Collections.Generic.List<BearPlayer>()
    let playerCount = ref 0

    while readerPlayers.Read() do
        incr playerCount
        let couldParse, mark = Int32.TryParse(readerPlayers.["mark"].ToString())
        bearPlayersList.Add(
            {
                bearId=Guid.Parse(readerPlayers.["bearId"].ToString());
                bearUsername= readerPlayers.["bearUsername"].ToString();
                bearAvatarId = Int32.Parse(readerPlayers.["bearAvatarId"].ToString());
                mark = mark;
                comment = readerPlayers.["comment"].ToString();
                isWaitingList = Int32.Parse(readerPlayers.["maxPlayers"].ToString())< playerCount.Value;
            }
        )

    
    let sqlGame = "Select gl.id, 
    gl.version,
    gl.name,
    gl.ownerId,
    gl.ownerBearName,
    gl.startDate,
    gl.location, 
    gl.currentState,
    Rooms.roomId,
    Rooms.version as roomVersion,
     COUNT(BearsInGamesToCount.bearId) AS nbPlayers,
     gl.maxPlayers
      from GamesList as gl 
    inner join Rooms on gl.id= Rooms.roomId
    LEFT OUTER  JOIN GamesBears         as BearsInGamesToCount on gl.id = BearsInGamesToCount.gameId  
    GROUP BY gl.id, gl.name,gl.ownerId,gl.ownerBearName,gl.startDate,gl.location,gl.currentState,gl.maxPlayers 
    HAVING gl.id = @gameId "
    use sqlCmdGame = new SQLiteCommand(sqlGame, connection) 

    let add (name:string, value: string) = 
        sqlCmdGame.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", filter.id.ToString())

    use readerGame = sqlCmdGame.ExecuteReader() 

    if (readerGame.Read()) then
        let ownerId = Guid.Parse(readerGame.["ownerId"].ToString())
        let isOwner = match ownerId with
                            | bearId -> true
                            | _ -> false
        let isPartOfGame = bearPlayersList.Any( fun b -> b.bearId =bearId)
        let couldParse,version = Int32.TryParse(readerGame.["version"].ToString())

        let gameDetail:GameDetail = {
            id=Guid.Parse(readerGame.["id"].ToString());
            name= readerGame.["name"].ToString();
            ownerId = ownerId;
            ownerUserName = readerGame.["ownerBearName"].ToString();
            startDate = DateTime.Parse(readerGame.["startDate"].ToString());
            location = readerGame.["location"].ToString();
            players = bearPlayersList;
            nbPlayers = Int32.Parse(readerGame.["nbPlayers"].ToString());
            maxPlayers = Int32.Parse(readerGame.["maxPlayers"].ToString());
            isJoinable = not <| isPartOfGame;
            isCancellable = isOwner;
            isOwner = isOwner;
            isAbandonnable = isPartOfGame;
            version = if couldParse then Nullable(version) else Nullable<int>()
        }
        
        readerGame.Dispose()
        sqlCmdGame.Dispose()
        connection.Dispose()
        GC.Collect()
        Some(gameDetail)
    else
        readerGame.Dispose()
        sqlCmdGame.Dispose()
        connection.Dispose()
        GC.Collect() 
        None        

let getGames bearId filter =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql = "SELECT        main.id, main.version, main.name, main.ownerId, main.ownerBearName, main.startDate, main.location, main.currentState, main.nbPlayers, main.maxPlayers, players.bearnames, 
                         CASE WHEN main.ownerId = @bearId THEN 'true' ELSE 'false' END AS isCancellable,
                         CASE WHEN main.ownerId = @bearId THEN 'true' ELSE 'false' END AS isOwner,
                         CASE WHEN bearInGame.IsPartOfGame =0 or bearInGame.IsPartOfGame is null THEN 'true' ELSE 'false' END AS  isJoinable,
                                         CASE WHEN bearInGame.IsPartOfGame =1 THEN 'true' ELSE 'false' END AS  isAbandonnable
                FROM            (SELECT        gl.id, gl.name, gl.ownerId, gl.ownerBearName, gl.startDate, gl.location, gl.currentState, COUNT(GamesBearsToCount.bearId) AS nbPlayers, 
                                                                    gl.maxPlayers, gl.version
                                          FROM            GamesList gl LEFT OUTER JOIN
                                                                    GamesBears GamesBearsToCount ON gl.id = GamesBearsToCount.gameId
                                          GROUP BY gl.id, gl.name, gl.ownerId, gl.ownerBearName, gl.startDate, gl.location, gl.currentState, gl.maxPlayers
                                          HAVING         (gl.currentState = 1)) main LEFT OUTER JOIN
                                             (SELECT        BearsSelection.gameId, GROUP_CONCAT(b.bearId, ',')  as BearIds, GROUP_CONCAT(b.bearUsername, ',') AS bearnames
                                               FROM            Bears b LEFT OUTER JOIN
                                                                         GamesBears BearsSelection ON BearsSelection.bearId = b.bearId
                                               GROUP BY BearsSelection.gameId) players ON main.id = players.gameId
                LEFT OUTER JOIN
                                (SELECT        gameId, COUNT(bearId) AS IsPartOfGame
                                FROM            GamesBears
                                WHERE        (bearId = @bearId)
                                GROUP BY gameId ) bearInGame 
                                ON main.id = bearInGame.gameId
                WHERE main.startDate>=@fromDate and main.startDate<=@toDate
                order by main.startDate Desc"

    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore
    
    add("@bearId", bearId.ToString())
    add("@fromDate", filter.From.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    add("@toDate", filter.To.ToString("yyyy-MM-ddTHH:mm:ssZ"))

    use reader = sqlCmd.ExecuteReader() 
    let gamesList = new System.Collections.Generic.List<Game>()

    while reader.Read() do
        let gameId = Guid.Parse(reader.["id"].ToString())
        let ownerId = reader.["ownerId"].ToString()
        let couldParse,version = Int32.TryParse(reader.["version"].ToString())
        gamesList.Add(
            {
                id=gameId;
                name= reader.["name"].ToString();
                ownerId = Guid.Parse(ownerId);
                ownerUserName = reader.["ownerBearName"].ToString();
                startDate = DateTime.Parse(reader.["startDate"].ToString());
                location = reader.["location"].ToString();
                players = reader.["bearnames"].ToString();
                nbPlayers = Int32.Parse(reader.["nbPlayers"].ToString());
                maxPlayers = Int32.Parse(reader.["maxPlayers"].ToString());
                isJoinable = bool.Parse(reader.["isJoinable"].ToString());
                isCancellable = bool.Parse(reader.["isCancellable"].ToString());
                isOwner =bool.Parse(reader.["isOwner"].ToString());
                isAbandonnable = bool.Parse(reader.["isAbandonnable"].ToString());
                version = if couldParse then Nullable(version) else Nullable<int>()
            }
        )
    
    reader.Dispose()
    sqlCmd.Dispose()
    connection.Dispose()
    GC.Collect()
    Some(gamesList)


let mapGameCmds (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (command:PetulantBear.Games.Commands) =    
    match command with 
    | Schedule(cmd) -> scheduleToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    | Join(cmd) -> joinToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) 
    | Cancel(cmd) -> cancelToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) 
    | Abandon(cmd) -> abandonToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) 
    | ChangeName(cmd) -> changeName sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    | ChangeStartDate(cmd) -> changeStartDate sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    | ChangeLocation(cmd) -> changeLocation sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    | KickPlayer(cmd) -> kickPlayer sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    | ChangeMaxPlayer(cmd) -> changeMaxPlayer sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) cmd
    

let mapAfterGamesCmds (sqlCmd:SQLiteCommand) (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  (command:AfterGames.Commands) =    
    match command with 
    | MarkBear(m) -> markBearToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) m
    | CommentBear(c) -> commentBearToDB sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) c
    




let signinToDB (id,version,bearId) ((socialId,cmd):string*SignIn) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()


    let sql = "Insert into Bears (bearId,bearUsername ,bearAvatarId  ) VALUES (@bearId, @bearUsername, @bearAvatarId); Insert into Users (socialId,bearId) VALUES (@socialId,@bearId)"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bearId.ToString())
    add("@socialId", socialId.ToString())
    add("@bearUsername", cmd.bearUsername)
    add("@bearAvatarId", cmd.bearAvatarId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore

    sqlCmd.Dispose()
    connection.Dispose()
    GC.Collect()

    Success(socialId,cmd)

let signinBearToDB bearId socialId (cmd:SignInBear) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()


    let sql = "Insert into Bears (bearId,bearUsername ,bearAvatarId  ) VALUES (@bearId, @bearUsername, @bearAvatarId); Insert into Users (socialId,bearId) VALUES (@socialId,@bearId); Insert into Authentication (authId, username, password) VALUES (@socialId,@bearUsername,@bearPassword)"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bearId.ToString())
    add("@socialId", socialId)
    add("@bearUsername", cmd.bearUsername)
    add("@bearAvatarId", cmd.bearAvatarId.ToString())
    add("@bearPassword", cmd.bearPassword)
    

    sqlCmd.ExecuteNonQuery() |> ignore
    
    sqlCmd.Dispose()
    connection.Dispose()
    GC.Collect()

    Success(socialId)

   

let getBears (filter:BearsFilter) = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId"
    use sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

    //let add (name:string, value: string) = 
    //    sqlCmdBear.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    //add("@bearId", filter.bearId.ToString())

    use reader = sqlCmdBear.ExecuteReader() 
    let bearList = new System.Collections.Generic.List<BearDetail>()

    while (reader.Read()) do
        bearList.Add( 
            {
                bearId=Guid.Parse(reader.["bearId"].ToString());
                bearUsername= reader.["bearUsername"].ToString();
                socialId = reader.["socialId"].ToString();
                bearAvatarId = Int32.Parse(reader.["bearAvatarId"].ToString());
            }
        )
    reader.Dispose()
    sqlCmdBear.Dispose()
    connection.Dispose()
    GC.Collect()
    Some(bearList)

let getBear (bearId:Guid) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId where b.bearId= @bearId"
    use sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

    let add (name:string, value: string) = 
        sqlCmdBear.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bearId.ToString())

    use reader = sqlCmdBear.ExecuteReader() 

    if (reader.Read()) then
        let bearFound: BearDetail = 
            {
                bearId=Guid.Parse(reader.["bearId"].ToString());
                bearUsername= reader.["bearUsername"].ToString();
                socialId = reader.["socialId"].ToString();
                bearAvatarId = Int32.Parse(reader.["bearAvatarId"].ToString());
            }
        reader.Dispose()
        sqlCmdBear.Dispose()
        connection.Dispose()
        GC.Collect()
        Some(bearFound)
    else
        reader.Dispose()
        sqlCmdBear.Dispose()
        connection.Dispose()
        GC.Collect() 
        None



let getBearFromSocialId (socialId:string) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId where u.socialId= @socialId"
    use sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

    let add (name:string, value: string) = 
        sqlCmdBear.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@socialId", socialId)

    use reader = sqlCmdBear.ExecuteReader() 

    if (reader.Read()) then
        let bearFound: BearSession = 
            {
                bearId=Guid.Parse(reader.["bearId"].ToString());
                socialId = reader.["socialId"].ToString();
                username = reader.["bearUsername"].ToString();
            }
        reader.Dispose()
        sqlCmdBear.Dispose()
        connection.Dispose()
        GC.Collect()
        Some(bearFound)
    else
        reader.Dispose()
        sqlCmdBear.Dispose()
        connection.Dispose()
        GC.Collect() 
        None


let login username password =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql =  "SELECT        Bears.bearId
                FROM            Authentication INNER JOIN
                                         Users ON Authentication.authId = Users.socialId INNER JOIN
                                         Bears ON Users.bearId = Bears.bearId
                WHERE        (Authentication.username = @username) AND (Authentication.password = @password)"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@username", username)
    add("@password", password)

    use reader = sqlCmd.ExecuteReader() 

    if (reader.Read()) then
        let bearId= reader.["bearId"].ToString()
        reader.Dispose()
        sqlCmd.Dispose()
        connection.Dispose()
        GC.Collect()
        bearId
        |> Guid.Parse
        |> getBear

    else 
        reader.Dispose()
        sqlCmd.Dispose()
        connection.Dispose()
        GC.Collect()
        None        

let getRoomDetail (filter :RoomFilter):RoomDetail option =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlMessages = "  select rm.roomId, rm.message,rm.typeMessage, b.bearId,b.bearUsername,b.bearAvatarId,u.socialId         from RoomMessages as rm         inner join  Bears as b on rm.bearId = b.bearId        inner join Users as u on b.bearId = u.bearId          where rm.roomId= @roomId"
    use sqlCmdMessages = new SQLiteCommand(sqlMessages, connection) 

    let add (name:string, value: string) = 
        sqlCmdMessages.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", filter.roomId.ToString())

    use readerMessages = sqlCmdMessages.ExecuteReader() 
    let messageList = new System.Collections.Generic.List<RoomMessageDetail>()

    while (readerMessages.Read()) do
        let tm= readerMessages.["typeMessage"].ToString()
        messageList.Add( 
            {
                roomId=Guid.Parse(readerMessages.["roomId"].ToString());
                bear= {
                        bearId=Guid.Parse(readerMessages.["bearId"].ToString());
                        bearUsername= readerMessages.["bearUsername"].ToString();
                        socialId = readerMessages.["socialId"].ToString();
                        bearAvatarId = Int32.Parse(readerMessages.["bearAvatarId"].ToString());
                }
                message = readerMessages.["message"].ToString();
                typeMessage = if tm="" then "text" else tm;
            }
        )

    let sqlRoom = "select r.roomId, r.name, r.version from Rooms as r where r.roomId = @roomId"
    use sqlCmdRoom = new SQLiteCommand(sqlRoom, connection) 

    let add (name:string, value: string) = 
        sqlCmdRoom.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", filter.roomId.ToString())

    use readerRoom = sqlCmdRoom.ExecuteReader() 

    if (readerRoom.Read()) then
        let couldParse,version = Int32.TryParse(readerRoom.["version"].ToString())
        let roomFound  = {
            roomId=Guid.Parse(readerRoom.["roomId"].ToString());
            name= readerRoom.["name"].ToString();
            version = if couldParse then Nullable(version) else Nullable<int>()
            messages = messageList;
        }
        readerRoom.Dispose()
        sqlCmdRoom.Dispose()
        readerMessages.Dispose()
        sqlCmdMessages.Dispose()
        connection.Dispose()
        GC.Collect()
        Some(roomFound)
    else
        readerRoom.Dispose()
        sqlCmdRoom.Dispose()
        readerMessages.Dispose()
        sqlCmdMessages.Dispose()
        connection.Dispose()
        GC.Collect() 
        None


    

let postMessage (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  (cmd:PostMessage) =
    sqlCmd.CommandText <-  "Insert into RoomMessages (roomId,bearId,message) VALUES (@roomId,@bearId, @message);"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", id.ToString())
    add("@bearId", bear.bearId.ToString())
    add("@message", cmd.message)


let mapRoomCmds (sqlCmd:SQLiteCommand) (id,version,bearId)  (command:PetulantBear.Rooms.Commands) =    
    match command with 
    | PostMessage(cmd) -> postMessage sqlCmd (id,version,bearId) cmd


let changeAvatarId (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  (cmd:ChangeAvatarId) =
    sqlCmd.CommandText <- "Update Bears set bearAvatarId =@bearAvatarId where  bearId=@bearId"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bear.bearId.ToString())
    add("@bearAvatarId", cmd.bearAvatarId.ToString())
    
    
let changePassword (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  (cmd:ChangePassword) =
    sqlCmd.CommandText <- "Update Authentication set password =@bearPassword where  username=@bearUsername"

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearUsername", bear.username)
    add("@bearPassword", cmd.bearPassword)
    

let changeUserName (sqlCmd:SQLiteCommand) ((id,version,bear):Guid*Nullable<int>*BearSession)  (cmd:ChangeUserName) =
    sqlCmd.CommandText <- "Update Bears set bearUsername =@bearUsername where  bearId=@bearId;Update Authentication set username =@bearUsername where  username=@oldBearUsername"
    
    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bear.bearId.ToString())
    add("@bearUsername", cmd.bearUsername)
    add("@oldBearUsername", bear.username)
    
    

let mapCurrentBearCmds sqlCmd (id,version,bearId)  (command:PetulantBear.CurrentBear.Commands) =    
    match command with 
    | ChangeAvatarId(cmd) -> changeAvatarId sqlCmd (id,version,bearId) cmd
    | ChangePassword(cmd) -> changePassword sqlCmd (id,version,bearId) cmd
    | ChangeUserName(cmd) -> changeUserName sqlCmd (id,version,bearId) cmd



let saveToDB map (((id,version,bear):Guid*Nullable<int>*BearSession):Guid*Nullable<int>*BearSession)  command =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()
    use sqlCmd = new SQLiteCommand( connection) 

    map sqlCmd ((id,version,bear):Guid*Nullable<int>*BearSession) command

    sqlCmd.ExecuteNonQuery() |> ignore

    sqlCmd.Dispose()
    connection.Dispose()
    GC.Collect()
    command




let resetDB ctx = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql = "Delete from  Users;Delete from  Bears;Delete from  GamesList;Delete from  GamesBears;Delete from  Rooms;Delete from  RoomMessages;Delete from  Rooms;Delete from  Authentication;Delete from commandProcessed; delete from projections;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    sqlCmd.ExecuteNonQuery() |> ignore

    sqlCmd.Dispose()
    connection.Dispose()
    GC.Collect()
    Suave.Http.Successful.OK "db deleted <a href='/logout'>log out </a>" ctx
    
