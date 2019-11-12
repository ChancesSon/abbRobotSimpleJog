MODULE Module1

    var socketdev temp_socket;
    var socketdev client_socket;
    VAR string receiveStr;
    
    var robtarget pTarget;
    VAR jointtarget jTarget;
    

    PROC main()
        CONST jointtarget joint1:=[[0,0,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
        CONST jointtarget joint2:=[[0,0,0,0,90,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
!        SocketClose temp_socket;
!        SocketClose client_socket;
!        SocketCreate temp_socket;
!        SocketBind temp_socket,"192.168.1.107",9492;
!        SocketListen temp_socket;
!        SocketAccept temp_socket,client_socket;
        
!        WHILE TRUE DO
!            SocketReceive client_socket\Str:=receiveStr;
!            IF(StrLen(receiveStr)>0) THEN
!                !
!            ENDIF        
!        ENDWHILE
         MoveAbsJ joint1,v1000,z50,tool0;
         MoveAbsJ joint2,v1000,z50,tool0;

        
    ENDPROC
    
    PROC MoveLinear()
        MoveL pTarget,v1000,fine,tool0;
    ENDPROC
    
    PROC moveJoint()
        MoveAbsJ jTarget,v1000,fine,tool0;
    ENDPROC
    

    
    
ENDMODULE