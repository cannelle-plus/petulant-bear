
BEGIN TRANSACTION;



--sql for users
-- SigIn Commands

Insert into Bears VALUES (@bearId, @bearUsername, @bearAvatarId); Insert into Users (socialId,bearId) VALUES (@socialId,@bearId)

Insert into Users VALUES ('1388221448136137', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fd');
Insert into Users VALUES ('1388221448136138', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fc');
Insert into Users VALUES ('1388221448136139', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5cc');
Insert into Users VALUES ('1388221448136130', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fa');

Insert into GamesList (id,name,ownerId,ownerBearName,startDate,location,maxPlayers) VALUES (@id, @name,@ownerId,@ownerUserName, @begins, @location, @maxPlayers); 
Insert into GamesBears (gameId,bearId)  VALUES (@id,@ownerId); 
Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name);

select main.*,players.bearnames from (Select gl.id, gl.name,gl.ownerId,gl.ownerBearName,gl.startDate,gl.location,         gl.currentState, COUNT(BearsInGamesToCount.bearId) AS nbPlayers,gl.maxPlayers from GamesList as gl INNER JOIN BearsInGames         as BearsInGamesToCount on gl.id = BearsInGamesToCount.gameId              GROUP BY gl.id, gl.name,gl.ownerId,gl.ownerBearName,gl.startDate,gl.location,gl.currentState,gl.maxPlayers          HAVING gl.currentState = 1) as main    inner join     (select BearsSelection.gameId , GROUP_CONCAT(b.bearUsername,',')         as bearnames from Bears as b INNER JOIN BearsInGames as BearsSelection on BearsSelection.bearId = b.bearId      Group By BearsSelection.gameId ) players on main.id = players.gameId



Insert into Bears VALUES ('fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fd', 'cannelle', '1');
Insert into Bears VALUES ('fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fc', 'aziz', '1');
Insert into Bears VALUES ('fd6c32a1-b8e2-429a-aff5-1ede3fd6f5cc', 'bond', '1');
Insert into Bears VALUES ('fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fa', 'yoann', '1');

Insert into GamesList VALUES ('7effa485-fa79-4b82-b04d-494ee879a29a','la moustache', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5cc', 'bond', '01/01/2015 10:00','PlaySoccer',  '8', 1);
Insert into GamesList VALUES ('50fc4592-c17a-45e9-ae52-0db92613ca8e', 'corde vs drogue', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fa', 'yoann', '01/01/2015 10:00','PlaySoccer', '10', 1);

Insert into GamesBears (gameId,bearId) VALUES ('7effa485-fa79-4b82-b04d-494ee879a29a', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5cc');
Insert into GamesBears (gameId,bearId) VALUES ('50fc4592-c17a-45e9-ae52-0db92613ca8e', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fa');
Insert into GamesBears (gameId,bearId) VALUES ('50fc4592-c17a-45e9-ae52-0db92613ca8e', 'fd6c32a1-b8e2-429a-aff5-1ede3fd6f5fc');

Insert into Rooms (roomId,name)  VALUES ('7effa485-fa79-4b82-b04d-494ee879a29a','salle de la moustache');
Insert into RoomMessages (roomId,bearId,message) VALUES ('7effa485-fa79-4b82-b04d-494ee879a29a','fd6c32a1-b8e2-429a-aff5-1ede3fd6f5cc', 'salut bande de naze!!!');


-- Insert into Grounds (groundId,name) VALUES ('6effa485-fa79-4b82-b04d-494ee879a29a','soccer 5');
-- Insert into Transmitters (transmitterId,groundId) VALUES ('6effa485-fa79-4b82-b04c-494ee879a29a','6effa485-fa79-4b82-b04d-494ee879a29a');
-- Insert into Receivers (receiverId,groundId) VALUES ('6effa485-fa79-4b81-b04c-494ee879a29a','6effa485-fa79-4b82-b04d-494ee879a29a');

  
COMMIT;





