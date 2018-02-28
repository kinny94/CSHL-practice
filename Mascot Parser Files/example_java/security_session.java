/*
##############################################################################
# file: security_session.java                                                #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2005 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
*/

/*
 This file needs to be compiled and put into the mascot/cgi directory.
 The file 'java_helper.pl' also needs to be copied to the mascot/cgi
 directory. Run from a browser: http://localhost/mascot/cgi/java_helper.pl
 */
 
import matrix_science.msparser.*;
import java.util.HashMap;

public class security_session {
    // 1st of all, load the .dll
    static {
        try {
            System.loadLibrary("msparserj");
        } catch (UnsatisfiedLinkError e) {
            System.err.println("Native code library failed to load. See the chapter on Dynamic Linking Problems in the SWIG Java documentation for help.\n" + e);
            e.printStackTrace();
            System.exit(1);
        }
    }
    
    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        // copy the parameters into a HashMap
        HashMap parameters = new HashMap();
        for(int i = 0; i < args.length; i += 2) {
            args[i+1] = args[i+1].replaceAll("-pipe-","\\|");
            args[i+1] = args[i+1].replaceAll("SYSTEM","\\(system\\)");
            parameters.put(args[i],args[i+1]);
        }
        ms_session session;
        
        System.out.println("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">");
        System.out.println("<HTML>");
        System.out.println("<HEAD>");
        System.out.println("<TITLE>Check Session</TITLE>");
        System.out.println("</head>\n");
        System.out.println("<body>\n");
        System.out.println("<p>This utility shows session information for when Mascot Security is enabled.<br>");
        System.out.println("You may be asked to give this information to a support engineer if you have security problems</p>");
        
        if(parameters.containsKey("COOKIE_MASCOT_SESSION")) System.out.println("Retrieved cookie value is: "+parameters.get("COOKIE_MASCOT_SESSION")+
        "<br>");
        if(parameters.containsKey("sessionID")) {
            System.out.println("Using passed sessionID: "+parameters.get("sessionID")+"<br>");
            session = new ms_session((String) parameters.get("sessionID"));
        } else {
            session = new ms_session();
        }
        
        if(session.isValid()) {
            System.out.println("<table border=\"1\">");
            System.out.print("  <tr><td>ID</td><td>"+session.getID()+"</td></tr>\n");
            System.out.print("  <tr><td>security enabled</td><td>"+session.isSecurityEnabled()+"</td></tr>\n");
            System.out.print("  <tr><td>last accessed</td><td>"+session.getLastAccessed()+"</td></tr>\n");
            System.out.print("  <tr><td>ip address</td><td>"+session.getIPAddress()+"</td></tr>\n");
            System.out.print("  <tr><td>user</td><td>"+session.getUserName()+"</td></tr>\n");
            System.out.print("  <tr><td>user ID</td><td>"+session.getUserID()+"</td></tr>\n");
            System.out.print("  <tr><td>full username</td><td>"+session.getFullUserName()+"</td></tr>\n");
            System.out.print("  <tr><td>email address</td><td>"+session.getEmailAddress()+"</td></tr>\n");
            System.out.print("  <tr><td>valid</td><td>"+session.isValid()+"</td></tr>\n");
            
            if(parameters.containsKey("ENV_ROMOTE_USER")) {
                System.out.print("  <tr><td>Web auth user</td><td>"+parameters.get("ENV_REMOTE_USER")+"</td></tr>");
            }
            System.out.print("</table><br><br>\n");
            showTasks(session);
            
        } else {
            System.out.print("Session is invalid<br>\n");
            displayWarningsAndErrors(session);
        }
        
        System.out.println("</body>\n");
        System.out.println("</html>\n");

    }
    
    private static void showTasks(ms_session session) {
        String[] param_desc = {"None","Integer =","Integer &lt;=","Integer &gt;=","Integer: one of", 
                   "Float =", "Float &lt;=", "Float &gt;=", "Float: one of", 
                   "String = ", "String: one of",
                   "User list"};
        ms_security_tasks tasks = session.getPermittedTasks();
        System.out.print("<b>Permitted tasks</b>\n");
        System.out.print("  <table align=\"left\" border=\"1\" cellspacing=\"0\" cellpadding=\"3\">\n");
        System.out.print("    <tr><td>Task</td><td nowrap>Param type</td><td>Parameter</td></tr>\n");
        for(int taskNo = 0; taskNo < tasks.getNumberOfTasks(); taskNo++) {
            ms_security_task currentTask = tasks.getTask(taskNo);
            int currentTT = currentTask.getType();
            System.out.print("      <tr>\n");
            System.out.print("        <td>"+currentTask.getDescription()+"</td>\n");
            System.out.print("        <td nowrap>"+param_desc[currentTT]+"</td>\n");
            System.out.print("        <td nowrap>"+currentTask.getAllParamsAsString()+"&nbsp;</td>\n");
            System.out.print("      </tr>\n");
        }
        System.out.print("  </table>\n");
    }
    
    private static void displayWarningsAndErrors(ms_session session) {
        if(!session.isValid()) {
            System.out.print("There were one or more errors:<br>\n");
        }
        
        ms_errs err = session.getErrorHandler();
        int numErrs = err.getNumberOfErrors();
        for(int i = 1; i <= numErrs; i++) {
            System.out.print(err.getErrorString(i)+"<br>\n");
        }
        session.clearAllErrors();
    }
    
}
