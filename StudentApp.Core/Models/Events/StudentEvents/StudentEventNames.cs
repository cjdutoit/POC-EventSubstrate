// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.StudentEvents
{
    public static class StudentEventNames
    {
        public const string StudentAdded = "StudentAddedEvent";
        public const string StudentModified = "StudentModifiedEvent";
        public const string StudentRemoved = "StudentRemovedEvent";
        public const string StudentEnrolled = "StudentEnrolledEvent";
        public const string TimetableGenerated = "TimetableGeneratedEvent";
        public const string WelcomeEmailSent = "WelcomeEmailSentEvent";
        public const string TimetableEmailSent = "TimetableEmailSentEvent";
        public const string ReactionFailed = "ReactionFailedEvent";
    }
}
