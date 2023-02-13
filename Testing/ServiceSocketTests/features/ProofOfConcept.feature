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

    Scenario: Can request configuration
        Given the Service is connected with handshake
        When configuration is requested
        Then a configuration response is received

    Scenario: Can request file configuration
        Given the Service is connected with handshake
        When file configuration is requested
        Then a configuration file response is received

    Scenario: Cannot request configuration without requestID
        Given the Service is connected with handshake
        When configuration is requested without a requestID
        Then a configuration error response is received

    Scenario: Can request tracking service status
        Given the Service is connected with handshake
        When tracking service status is requested
        Then a tracking service status response is received