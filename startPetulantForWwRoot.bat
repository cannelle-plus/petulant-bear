start "getEventStore" "paket-files/download.geteventstore.com/EventStore.ClusterNode.exe" --db ./data/db --log ./data/logs --run-projections=all
timeout 5 > NUL
start "petulantBear" "bin/PetulantBear/PetulantBear.exe" --dbConnection="Data Source=%CD%\src\db\db\Bear2Bear.db;Version=3" --rootPath="%CD%\src\wwwroot" --ipAddress="127.0.0.1" --port=8084 --urlSite="http://localhost:8084" --eventStoreConnectionString="tcp://127.0.0.1:1113" --eventStoreClientIp="127.0.0.1" --eventStoreClientPort=2113
