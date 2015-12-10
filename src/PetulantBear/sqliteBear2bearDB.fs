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

let scheduleToDB connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:ScheduleGame) =
    let sql = "Insert into GamesList (id,name,ownerId,ownerBearName,startDate,location,maxPlayers) VALUES (@id, @name,@ownerId,@ownerUserName, @begins, @location, @maxPlayers); Insert into GamesBears (gameId,bearId)  VALUES (@id,@ownerId); Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name);"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@name", cmd.name)
    add("@ownerId", bear.bearId.ToString())
    add("@ownerUserName", bear.username)
    add("@begins", cmd.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    add("@location", cmd.location.ToString())
    add("@maxPlayers", cmd.maxPlayers.ToString())
    sqlCmd

let joinToDB connection ((id,version,bear):Guid*int*BearSession)  =
    let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId; Insert into GamesBears (gameId,bearId)  VALUES (@id,@bearId)"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", bear.bearId.ToString())
    sqlCmd    

let cancelToDB connection ((id,version,bear):Guid*int*BearSession)  =
    let sql = "update GamesList set currentState=0 where id=@id"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    sqlCmd    

let abandonToDB connection ((id,version,bear):Guid*int*BearSession)  =
    let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", bear.bearId.ToString())
    sqlCmd    

let changeName connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:Games.Contracts.ChangeName) =
    let sql = "Update  GamesList set name= @name where id=@id"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@name", cmd.name)
    sqlCmd

let changeLocation connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:Games.Contracts.ChangeLocation) =
    let sql = "Update  GamesList set location= @location where id=@id"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@location", cmd.location)
    sqlCmd

let changeStartDate connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:Games.Contracts.ChangeStartDate) =
    let sql = "Update  GamesList set startDate= @startDate where id=@id"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@startDate", cmd.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    sqlCmd

let changeMaxPlayer connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:Games.Contracts.ChangeMaxPlayer) =
    let sql = "Update  GamesList set maxPlayers= @maxPlayers where id=@id"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@maxPlayers", cmd.maxPlayers.ToString())
    sqlCmd

let kickPlayer connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (cmd:Games.Contracts.KickPlayer) =
    let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@bearId", cmd.kickedBearId.ToString())
    sqlCmd



let retrieveGamesBears connection gameId bearId =
     //retrieving information for the game if existing
    let sqlGamesBears = "select gb.bearId, gb.gameId, gb.mark, gb.comment from GamesBears as gb  where  gb.gameId=@gameId and gb.bearId=@bearId  "
    let sqlCmdGamesBears = new SQLiteCommand(sqlGamesBears, connection) 

    let add (name:string, value: string) = 
        sqlCmdGamesBears.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", gameId)
    add("@bearId", bearId)

    let readerGamesBears = sqlCmdGamesBears.ExecuteReader() 

    let mutable mark = ""
    let mutable comment = ""
    
    if readerGamesBears.Read() then
        mark <- readerGamesBears.["mark"].ToString()
        comment <- readerGamesBears.["comment"].ToString()

    mark,comment


let markBearToDB connection ((id,version,bear):Guid*int*BearSession) (cmd:MarkBear)=

    let (mark,comment) = retrieveGamesBears connection (id.ToString()) (bear.bearId.ToString())

    let sql = "Update GamesBears  set mark=@mark where BearId=@bearId and gameId=@gameId;"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", id.ToString())
    add("@bearId", cmd.bearId.ToString())
    add("@mark", cmd.mark.ToString())
    
    sqlCmd


let commentBearToDB connection ((id,version,bear):Guid*int*BearSession)  (cmd:CommentBear) =

    let (mark,comment) = retrieveGamesBears connection (id.ToString()) (bear.bearId.ToString())

    let sql = "Update GamesBears  set comment=@comment where BearId=@bearId and gameId=@gameId;"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", id.ToString())
    add("@bearId", cmd.bearId.ToString())
    add("@comment", cmd.comment)
    sqlCmd


let getGame bearId (filter:GamesFilter) = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    //retrieving players for the game
    let sqlPlayers = "SELECT        b.bearId, b.bearUsername, b.bearAvatarId, BearsSelection.mark, BearsSelection.comment, GamesList.maxPlayers
                        FROM            Bears b INNER JOIN
                                                 GamesBears BearsSelection ON BearsSelection.bearId = b.bearId INNER JOIN
                                                 GamesList ON GamesList.id = BearsSelection.gameId
                        WHERE        BearsSelection.gameId =@gameId  "
    let sqlCmdPlayers = new SQLiteCommand(sqlPlayers, connection) 

    let add (name:string, value: string) = 
        sqlCmdPlayers.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", filter.id.ToString())


    let readerPlayers = sqlCmdPlayers.ExecuteReader() 
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
    gl.name,
    gl.ownerId,
    gl.ownerBearName,
    gl.startDate,
    gl.location, 
    gl.currentState,
     COUNT(BearsInGamesToCount.bearId) AS nbPlayers,
     gl.maxPlayers
      from GamesList as gl 
    LEFT OUTER  JOIN GamesBears         as BearsInGamesToCount on gl.id = BearsInGamesToCount.gameId  
    GROUP BY gl.id, gl.name,gl.ownerId,gl.ownerBearName,gl.startDate,gl.location,gl.currentState,gl.maxPlayers 
    HAVING gl.id = @gameId "
    let sqlCmdGame = new SQLiteCommand(sqlGame, connection) 

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
        }
        Some(gameDetail)
    else None        

let getGames bearId filter =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql = "SELECT        main.id, main.name, main.ownerId, main.ownerBearName, main.startDate, main.location, main.currentState, main.nbPlayers, main.maxPlayers, players.bearnames, 
                         CASE WHEN main.ownerId = @bearId THEN 'true' ELSE 'false' END AS isCancellable,
                         CASE WHEN main.ownerId = @bearId THEN 'true' ELSE 'false' END AS isOwner,
                         CASE WHEN bearInGame.IsPartOfGame =0 or bearInGame.IsPartOfGame is null THEN 'true' ELSE 'false' END AS  isJoinable,
                                         CASE WHEN bearInGame.IsPartOfGame =1 THEN 'true' ELSE 'false' END AS  isAbandonnable
                FROM            (SELECT        gl.id, gl.name, gl.ownerId, gl.ownerBearName, gl.startDate, gl.location, gl.currentState, COUNT(GamesBearsToCount.bearId) AS nbPlayers, 
                                                                    gl.maxPlayers
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

    let sqlCmd = new SQLiteCommand(sql, connection) 

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
            }
        )
    Some(gamesList)


let mapGameCmds connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (command:PetulantBear.Games.Commands) =    
    match command with 
    | Schedule(cmd) -> scheduleToDB connection ((id,version,bear):Guid*int*BearSession) cmd
    | Join -> joinToDB connection ((id,version,bear):Guid*int*BearSession) 
    | Cancel -> cancelToDB connection ((id,version,bear):Guid*int*BearSession) 
    | Abandon -> abandonToDB connection ((id,version,bear):Guid*int*BearSession) 
    | ChangeName(cmd) -> changeName connection ((id,version,bear):Guid*int*BearSession) cmd
    | ChangeStartDate(cmd) -> changeStartDate connection ((id,version,bear):Guid*int*BearSession) cmd
    | ChangeLocation(cmd) -> changeLocation connection ((id,version,bear):Guid*int*BearSession) cmd
    | KickPlayer(cmd) -> kickPlayer connection ((id,version,bear):Guid*int*BearSession) cmd
    | ChangeMaxPlayer(cmd) -> changeMaxPlayer connection ((id,version,bear):Guid*int*BearSession) cmd
    

let mapAfterGamesCmds connection (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  (command:AfterGames.Commands) =    
    match command with 
    | MarkBear(m) -> markBearToDB connection ((id,version,bear):Guid*int*BearSession) m
    | CommentBear(c) -> commentBearToDB connection ((id,version,bear):Guid*int*BearSession) c
    




let signinToDB (id,version,bearId) ((socialId,cmd):string*SignIn) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()


    let sql = "Insert into Bears VALUES (@bearId, @bearUsername, @bearAvatarId); Insert into Users (socialId,bearId) VALUES (@socialId,@bearId)"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bearId.ToString())
    add("@socialId", socialId.ToString())
    add("@bearUsername", cmd.bearUsername)
    add("@bearAvatarId", cmd.bearAvatarId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore

    Success(socialId,cmd)

let signinBearToDB bearId socialId (cmd:SignInBear) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()


    let sql = "Insert into Bears VALUES (@bearId, @bearUsername, @bearAvatarId); Insert into Users (socialId,bearId) VALUES (@socialId,@bearId); Insert into Authentication (authId, username, password) VALUES (@socialId,@bearUsername,@bearPassword)"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bearId.ToString())
    add("@socialId", socialId)
    add("@bearUsername", cmd.bearUsername)
    add("@bearAvatarId", cmd.bearAvatarId.ToString())
    add("@bearPassword", cmd.bearPassword)
    

    sqlCmd.ExecuteNonQuery() |> ignore

    Success(socialId)

   

let getBears (filter:BearsFilter) = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId"
    let sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

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
    Some(bearList)

let getBear (bearId:Guid) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId where b.bearId= @bearId"
    let sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

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
        Some(bearFound)
    else None



let getBearFromSocialId (socialId:string) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlBear = "select b.bearId,b.bearUsername,b.bearAvatarId,u.socialId from Bears as b inner join Users as u on b.bearId = u.bearId where u.socialId= @socialId"
    let sqlCmdBear = new SQLiteCommand(sqlBear, connection) 

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
        Some(bearFound)
    else None


let login username password =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql =  "SELECT        Bears.bearId
                FROM            Authentication INNER JOIN
                                         Users ON Authentication.authId = Users.socialId INNER JOIN
                                         Bears ON Users.bearId = Bears.bearId
                WHERE        (Authentication.username = @username) AND (Authentication.password = @password)"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@username", username)
    add("@password", password)

    use reader = sqlCmd.ExecuteReader() 

    if (reader.Read()) then
        reader.["bearId"].ToString()
        |> Guid.Parse
        |> getBear

    else 
        None        

let getRoomDetail (filter :RoomFilter) =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlMessages = "  select rm.roomId, rm.message, b.bearId,b.bearUsername,b.bearAvatarId,u.socialId         from RoomMessages as rm         inner join  Bears as b on rm.bearId = b.bearId        inner join Users as u on b.bearId = u.bearId          where rm.roomId= @roomId"
    let sqlCmdMessages = new SQLiteCommand(sqlMessages, connection) 

    let add (name:string, value: string) = 
        sqlCmdMessages.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", filter.roomId.ToString())

    use readerMessages = sqlCmdMessages.ExecuteReader() 
    let messageList = new System.Collections.Generic.List<RoomMessageDetail>()

    while (readerMessages.Read()) do
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
            }
        )

    let sqlRoom = "select r.roomId, r.name from Rooms as r where r.roomId = @roomId"
    let sqlCmdRoom = new SQLiteCommand(sqlRoom, connection) 

    let add (name:string, value: string) = 
        sqlCmdRoom.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", filter.roomId.ToString())

    use readerRoom = sqlCmdRoom.ExecuteReader() 


    if (readerRoom.Read()) then
        let roomFound  = {
            roomId=Guid.Parse(readerRoom.["roomId"].ToString());
            name= readerRoom.["name"].ToString();
            messages = messageList;
        }
        Some(roomFound)
    else None


    

let postMessage connection ((id,version,bear):Guid*int*BearSession)  (cmd:PostMessage) =
    let sql = "Insert into RoomMessages (roomId,bearId,message) VALUES (@roomId,@bearId, @message);"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@roomId", id.ToString())
    add("@bearId", bear.bearId.ToString())
    add("@message", cmd.message)
    
    sqlCmd

let mapRoomCmds connection (id,version,bearId)  (command:PetulantBear.Rooms.Commands) =    
    match command with 
    | PostMessage(cmd) -> postMessage connection (id,version,bearId) cmd


let changeAvatarId connection ((id,version,bear):Guid*int*BearSession)  (cmd:ChangeAvatarId) =
    let sql = "Update Bears set bearAvatarId =@bearAvatarId where  bearId=@bearId"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bear.bearId.ToString())
    add("@bearAvatarId", cmd.bearAvatarId.ToString())
    
    sqlCmd
    
let changePassword connection ((id,version,bear):Guid*int*BearSession)  (cmd:ChangePassword) =
    let sql = "Update Authentication set password =@bearPassword where  username=@bearUsername"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearUsername", bear.username)
    add("@bearPassword", cmd.bearPassword)
    
    sqlCmd

let changeUserName connection ((id,version,bear):Guid*int*BearSession)  (cmd:ChangeUserName) =
    let sql = "Update Bears set bearUsername =@bearUsername where  bearId=@bearId;Update Authentication set username =@bearUsername where  username=@oldBearUsername"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", bear.bearId.ToString())
    add("@bearUsername", cmd.bearUsername)
    add("@oldBearUsername", bear.username)
    
    sqlCmd

let mapCurrentBearCmds connection (id,version,bearId)  (command:PetulantBear.CurrentBear.Commands) =    
    match command with 
    | ChangeAvatarId(cmd) -> changeAvatarId connection (id,version,bearId) cmd
    | ChangePassword(cmd) -> changePassword connection (id,version,bearId) cmd
    | ChangeUserName(cmd) -> changeUserName connection (id,version,bearId) cmd



let saveToDB map (((id,version,bear):Guid*int*BearSession):Guid*int*BearSession)  command =
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sqlCmd :SQLiteCommand = map connection ((id,version,bear):Guid*int*BearSession) command

    sqlCmd.ExecuteNonQuery() |> ignore

    command




let resetDB ctx = 
    use connection = new SQLiteConnection(dbConnection)
    connection.Open()

    let sql = "Delete from  Users;Delete from  Bears;Delete from  GamesList;Delete from  GamesBears;Delete from  Rooms;Delete from  RoomMessages;Delete from  Rooms;Delete from  Authentication;"
    let sqlCmd = new SQLiteCommand(sql, connection) 

    sqlCmd.ExecuteNonQuery() |> ignore
    Suave.Http.Successful.OK "db deleted <a href='/logout'>log out </a>" ctx
    
