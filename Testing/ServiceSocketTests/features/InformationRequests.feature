Feature: Proof the tests are working

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

    Scenario: Can set configuration
        Given the Service is connected with handshake
        When the configuration is set
        Then a configuration response is received with InteractionDistance