# acquire -> .har extractor/organizer

    start
        input .har file
        validate .har file
        kill process if .har file is invalid

    acquiring
        process .har file
        parse the JSON
        extract requests
        for each request:
            extract resource path, method, status, and size
            create folder for each request based on status and method
            save request headers
            save response headers
            save resource size and URL information
            log the acquired request

    formatting
        output organized folder structure
        that's done

    operational
        output
            {date-time} {filename}
                request_{n} = {size} bytes
                    request_headers.txt
                    response_headers.txt
                    info.txt
