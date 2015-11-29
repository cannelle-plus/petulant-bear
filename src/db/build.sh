if [ ! -d "src/db/temp" ]
then
	mkdir src/db/temp 
fi

if [ ! -d "src/db/db" ]
then
	mkdir src/db/db
fi

rm -r src/db/db/Bear2Bear.db
rm -r src/db/temp/script.db

sqlite3 src/db/db/Bear2Bear.db < src/db/scripts/authenticationScript.sql


