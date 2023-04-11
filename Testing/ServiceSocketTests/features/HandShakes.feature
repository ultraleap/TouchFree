Feature: Can Perform HandShakes

    Scenario: Prove a connection to the server was made
        Given the Service is running

    Scenario: Can complete handshake
        Given the Service is connected
        When a handshake message is sent 
        Then a handshake response is received

    Scenario: Can complete handshake with older version
        Given the Service is connected
        When a handshake message is sent with a "older" "minor" version
        Then a handshake response is received with a warning for versions

    Scenario: Can complete handshake with a newer patch version
        Given the Service is connected
        When a handshake message is sent with a "newer" "patch" version
        Then a handshake response is received with a warning for versions

    Scenario: Cannot complete handshake with newer minor version
        Given the Service is connected
        When a handshake message is sent with a "newer" "minor" version
        Then a handshake response is received with a version error

    Scenario: Cannot complete handshake with newer major version
        Given the Service is connected
        When a handshake message is sent with a "newer" "major" version
        Then a handshake response is received with a version error

    Scenario: Cannot complete handshake with older major version
        Given the Service is connected
        When a handshake message is sent with a "older" "major" version
        Then a handshake response is received with a version error