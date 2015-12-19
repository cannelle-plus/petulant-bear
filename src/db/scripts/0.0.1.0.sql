--sql for authentication

BEGIN TRANSACTION;


CREATE TABLE if not exists Users (socialId text not null, bearId text not null);
CREATE TABLE if not exists Authentication (authId text not null, username text not null, password text not null);
CREATE TABLE if not exists Bears (bearId text not null, bearUsername text not null, bearAvatarId text not null);

-- currentState
--  premier bit 0/1 -> isCancelled
CREATE TABLE if not exists GamesList (id text primary key not null,  name text not null, ownerId text not null,ownerBearName text not null, startDate text not null, location text not null,  maxPlayers text not null, currentState int not null default 1);
CREATE TABLE if not exists GamesBears(gameId text not null,bearId text not null, mark text , comment text  );


-- CREATE TABLE Projections (name text not null, messageIdProcessed text not null);

CREATE TABLE if not exists Rooms (roomId text primary key not null,  name text not null);
CREATE TABLE if not exists RoomMessages (roomId text not null,  bearId text not null, message text not null);


-- CREATE TABLE ReceivedSignals(transmitterId text not null, receiverId text not null, signalStrength text not null, receptionDate text not null);
-- CREATE TABLE Grounds(groundId text primary key not null, name text not null);
-- CREATE TABLE Transmitters(transmitterId text primary key not null,groundId text not null);
-- CREATE TABLE Receivers(receiverId text primary key not null,groundId text not null);
-- CREATE TABLE Calibrations(transmitterId text not null,receiverId text not null, distance text not null, startDate text not null, stopDate text );
-- CREATE TABLE Adjustments(transmitterId text not null,receiverId text not null, n text not null, A text not null);






COMMIT;
