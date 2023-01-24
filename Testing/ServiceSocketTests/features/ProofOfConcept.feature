Feature: Proof the tests are working

    Scenario: Prove a connection to the server was made
        Given the Service is running

    Scenario: Can complete handshake
        Given the Service is connected
        When a handshake message is sent
        Then a handshake response is received

    Scenario: Can request service status
        Given the Service is connected with handshake
        When service status is requested
        Then a service status response is received