#!/bin/sh
curl -s http://192.168.9.109:8888/Patient | jq ".entry[1:] | .[].fullUrl" -r | tr '\n\r' ' ' | xargs curl -X DELETE
curl -s http://192.168.9.109:8888/Practitioner | jq ".entry[1:] | .[].fullUrl" -r | tr '\n\r' ' ' | xargs curl -X DELETE
curl -s http://192.168.9.109:8888/Observation | jq ".entry[1:] | .[].fullUrl" -r | tr '\n\r' ' ' | xargs curl -X DELETE
curl -s http://192.168.9.109:8888/ProcedureRequest | jq ".entry[1:] | .[].fullUrl" -r | tr '\n\r' ' ' | xargs curl -X DELETE
curl -s http://192.168.9.109:8888/Specimen | jq ".entry[1:] | .[].fullUrl" -r | tr '\n\r' ' ' | xargs curl -X DELETE

