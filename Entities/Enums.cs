namespace a2_tp3_job_connect.Entities;

public enum UserPermission
{
    Candidate = 1,
    Recruiter = 2,
    Manager = 3,
    Administrator = 4
}

public enum CompanyUserRole
{
    Recruiter = 1,
    Manager = 2
}

public enum JobStatus
{
    Draft = 1,
    PendingApproval = 2,
    Published = 3,
    Closed = 4,
    Rejected = 5
}

public enum WorkModel
{
    OnSite = 1,
    Hybrid = 2,
    Remote = 3
}

public enum JobLevel
{
    Internship = 1,
    Junior = 2,
    Mid = 3,
    Senior = 4,
    Specialist = 5,
    Leadership = 6
}

public enum SkillRequirementType
{
    Required = 1,
    Differential = 2
}

public enum ApplicationStatus
{
    Received = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Withdrawn = 5
}

public enum DocumentType
{
    ResumePdf = 1,
    Certificate = 2,
    Portfolio = 3,
    Other = 4
}

public enum NotificationType
{
    NewApplication = 1,
    StageAdvanced = 2,
    CandidateRejected = 3,
    JobApproved = 4,
    JobClosed = 5
}
