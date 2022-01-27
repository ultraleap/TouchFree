#!/bin/sh

PIDFILE=/var/run/haptic-server.pid
DAEMON="./TouchFree_Service"

if [[ $1 == "start" ]]; then
    export LD_LIBRARY_PATH=.
    start-stop-daemon --start \
        --pidfile $PIDFILE --make-pidfile \
        --background --exec $DAEMON
elif [[ $1 == "stop" ]]; then
    start-stop-daemon --stop --exec $DAEMON --pidfile $PIDFILE
fi