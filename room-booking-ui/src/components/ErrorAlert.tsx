interface ErrorAlertProps {
  error: unknown;
  onDismiss?: () => void;
  onRetry?: () => void;
}

export default function ErrorAlert({ error, onDismiss, onRetry }: ErrorAlertProps) {
  if (!error) return null;
  const message = error instanceof Error ? error.message : "The request failed.";

  return (
    <div className="error-alert" role="alert">
      <span>{message}</span>
      {onRetry && <button type="button" onClick={onRetry}>Retry</button>}
      {onDismiss && <button type="button" aria-label="Dismiss error" onClick={onDismiss}>×</button>}
    </div>
  );
}
